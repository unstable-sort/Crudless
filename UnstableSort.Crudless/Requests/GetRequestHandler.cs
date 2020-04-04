using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using UnstableSort.Crudless.Common.ServiceProvider;
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

        private readonly ServiceProviderContainer _container;

        public GetRequestHandler(IEntityContext context,
            ServiceProviderContainer container, 
            CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            _container = container;

            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public Task<Response<TOut>> HandleAsync(TRequest request, CancellationToken token)
        {
            var provider = _container.CreateProvider();

            ApplyConfiguration(request);

            return HandleWithErrorsAsync(request, provider, token, _HandleAsync);
        }

        private async Task<TOut> _HandleAsync(TRequest request,
            IServiceProvider provider, 
            CancellationToken token)
        {
            var mapper = provider.ProvideInstance<IMapper>();

            await request.RunRequestHooks(RequestConfig, provider, token).Configure();

            var entities = Context.Set<TEntity>()
                .FilterWith(request, RequestConfig, provider)
                .SelectWith(request, RequestConfig);

            var result = default(TOut);

            if (Options.UseProjection)
            {
                result = await entities.ProjectSingleOrDefaultAsync<TEntity, TOut>(mapper.ConfigurationProvider, token).Configure();
                token.ThrowIfCancellationRequested();
                
                if (result == null)
                {
                    if (RequestConfig.ErrorConfig.FailedToFindInGetIsError)
                        throw new FailedToFindException { EntityTypeProperty = typeof(TEntity) };

                    result = await RequestConfig.GetDefaultFor<TEntity>()
                        .CreateResult<TRequest, TEntity, TOut>(request, RequestConfig, provider, token)
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

                await request.RunEntityHooks<TEntity>(RequestConfig, provider, entity, token).Configure();

                result = await entity
                    .CreateResult<TRequest, TEntity, TOut>(request, RequestConfig, provider, token)
                    .Configure();
            }

            token.ThrowIfCancellationRequested();
            
            return await request.RunResultHooks(RequestConfig, provider, result, token).Configure();
        }
    }
}
