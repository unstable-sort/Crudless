using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Exceptions;
using UnstableSort.Crudless.Requests;
using IServiceProvider = UnstableSort.Crudless.Common.ServiceProvider.IServiceProvider;

namespace UnstableSort.Crudless.Extensions
{
    internal static class EntityExtensions
    {
        private const string GenericCreateResultError 
            = "A request 'result creator' failed while processing the request.";

        private static bool IsNonCancellationFailure(Exception e)
            => !(e is OperationCanceledException);

        internal static async Task<TResult> CreateResult<TRequest, TEntity, TResult>(this TEntity entity,
            TRequest request,
            IRequestConfig config,
            IServiceProvider provider,
            CancellationToken token)
            where TRequest : ICrudlessRequest
            where TEntity : class
        {
            var createResult = config.GetResultCreatorFor<TEntity, TResult>();
            var context = new RequestContext<TRequest>
            {
                Request = request,
                ServiceProvider = provider
            }.Box();

            try
            {
                var result = await createResult(context, entity, token).Configure();
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

        internal static async Task<TResult[]> CreateResults<TRequest, TEntity, TResult>(this TEntity[] entities,
            TRequest request,
            IRequestConfig config,
            IServiceProvider provider,
            CancellationToken token)
            where TRequest : ICrudlessRequest
            where TEntity : class
        {
            var createResult = config.GetResultCreatorFor<TEntity, TResult>();
            var context = new RequestContext<TRequest>
            {
                Request = request,
                ServiceProvider = provider
            }.Box();

            try
            {
                var results = await Task.WhenAll(entities.Select(x => createResult(context, x, token))).Configure();
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
