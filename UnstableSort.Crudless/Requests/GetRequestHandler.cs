using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Context;
using UnstableSort.Crudless.Exceptions;
using UnstableSort.Crudless.Extensions;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Requests
{
    internal class GetRequestHandler<TRequest, TEntity, TOut>
        : CrudlessRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : IGetRequest<TEntity, TOut>, ICrudlessRequest<TEntity, TOut>
    {
        protected readonly RequestOptions Options;

        public GetRequestHandler(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public Task<Response<TOut>> HandleAsync(TRequest request, CancellationToken token)
        {
            return HandleWithErrorsAsync(request, token, _HandleAsync);
        }

        private async Task<TOut> _HandleAsync(TRequest request, CancellationToken token)
        {
            await request.RunRequestHooks(RequestConfig, token).Configure();

            var entities = Context.Set<TEntity>()
                .FilterWith(request, RequestConfig)
                .SelectWith(request, RequestConfig);

            var result = default(TOut);

            if (Options.UseProjection)
            {
                result = await entities.ProjectSingleOrDefaultAsync<TEntity, TOut>(token).Configure();
                token.ThrowIfCancellationRequested();
                
                if (result == null)
                {
                    if (RequestConfig.ErrorConfig.FailedToFindInGetIsError)
                        throw new FailedToFindException { EntityTypeProperty = typeof(TEntity) };

                    result = await RequestConfig.GetDefaultFor<TEntity>()
                        .CreateResult<TEntity, TOut>(RequestConfig, token)
                        .Configure();
                }
            }
            else
            {
                var entity = await entities.SingleOrDefaultAsync(token).Configure();
                token.ThrowIfCancellationRequested();

                if (entity == null)
                {
                    if (RequestConfig.ErrorConfig.FailedToFindInGetIsError)
                        throw new FailedToFindException { EntityTypeProperty = typeof(TEntity) };

                    entity = RequestConfig.GetDefaultFor<TEntity>();
                }

                await request.RunEntityHooks<TEntity>(RequestConfig, entity, token).Configure();

                result = await entity
                    .CreateResult<TEntity, TOut>(RequestConfig, token)
                    .Configure();
            }

            token.ThrowIfCancellationRequested();
            
            return await request.RunResultHooks(RequestConfig, result, token).Configure();
        }
    }
}
