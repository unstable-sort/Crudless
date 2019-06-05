using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Exceptions;
using UnstableSort.Crudless.Requests;

namespace UnstableSort.Crudless.Extensions
{
    internal static class AuditedRequestExtensions
    {
        private static string GenericHookError(string hookType)
            => $"A request '{hookType} hook' failed while processing the request.";

        private static bool IsNonCancellationFailure(Exception e)
            => !(e is OperationCanceledException);
        
        internal static async Task RunAuditHooks<TEntity>(this ICrudlessRequest request,
            IRequestConfig config,
            object oldEntity,
            object newEntity,
            CancellationToken ct)
            where TEntity : class
        {
            var hooks = config.GetAuditHooksFor<TEntity>();

            foreach (var hook in hooks)
            {
                try
                {
                    await hook.Run(request, oldEntity, newEntity, ct).Configure();
                    ct.ThrowIfCancellationRequested();
                }
                catch (Exception e) when (IsNonCancellationFailure(e))
                {
                    throw new HookFailedException(GenericHookError("audit"), e)
                    {
                        HookProperty = hook
                    };
                }
            }
        }

        internal static async Task RunAuditHooks<TEntity>(this ICrudlessRequest request,
            IRequestConfig config,
            IEnumerable<object> oldEntities,
            IEnumerable<object> newEntities,
            CancellationToken ct)
            where TEntity : class
        {
            var hooks = config.GetAuditHooksFor<TEntity>();
            var entities = oldEntities.Zip(newEntities, (oldEntity, newEntity) => (oldEntity, newEntity));

            foreach (var (oldEntity, newEntity) in entities)
            {
                foreach (var hook in hooks)
                {
                    try
                    {
                        await hook.Run(request, oldEntity, newEntity, ct).Configure();
                        ct.ThrowIfCancellationRequested();
                    }
                    catch (Exception e) when (IsNonCancellationFailure(e))
                    {
                        throw new HookFailedException(GenericHookError("audit"), e)
                        {
                            HookProperty = hook
                        };
                    }
                }
            }
        }
    }
}
