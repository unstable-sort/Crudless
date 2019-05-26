using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Exceptions;

namespace UnstableSort.Crudless.Extensions
{
    internal static class EntityExtensions
    {
        private const string GenericCreateResultError 
            = "A request 'result creator' failed while processing the request.";

        private static bool IsNonCancellationFailure(Exception e)
            => !(e is OperationCanceledException);

        internal static async Task<TResult> CreateResult<TEntity, TResult>(this TEntity entity,
            IRequestConfig config,
            CancellationToken token)
            where TEntity : class
        {
            var createResult = config.GetResultCreatorFor<TEntity, TResult>();

            try
            {
                var result = await createResult(entity, token).Configure();
                token.ThrowIfCancellationRequested();

                return result;
            }
            catch (Exception e) when (IsNonCancellationFailure(e))
            {
                throw new CreateResultFailedException(GenericCreateResultError, e)
                {
                    EntityProperty = entity
                };
            }
        }

        internal static async Task<TResult[]> CreateResults<TEntity, TResult>(this TEntity[] entities,
            IRequestConfig config,
            CancellationToken token)
            where TEntity : class
        {
            var createResult = config.GetResultCreatorFor<TEntity, TResult>();

            try
            {
                var results = await Task.WhenAll(entities.Select(x => createResult(x, token))).Configure();
                token.ThrowIfCancellationRequested();

                return results;
            }
            catch (Exception e) when (IsNonCancellationFailure(e))
            {
                throw new CreateResultFailedException(GenericCreateResultError, e)
                {
                    EntityProperty = entities
                };
            }
        }
    }
}
