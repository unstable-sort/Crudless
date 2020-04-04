using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Context;
using UnstableSort.Crudless.Exceptions;
using UnstableSort.Crudless.Extensions;
using UnstableSort.Crudless.Mediator;

using IServiceProvider = UnstableSort.Crudless.Common.ServiceProvider.IServiceProvider;

namespace UnstableSort.Crudless.Requests
{
    internal class GetAllRequestHandler<TRequest, TEntity, TOut>
        : CrudlessRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, GetAllResult<TOut>>
        where TEntity : class
        where TRequest : IGetAllRequest<TEntity, TOut>, ICrudlessRequest<TEntity, TOut>
    {
        private readonly ServiceProviderContainer _container;

        protected readonly RequestOptions Options;

        public GetAllRequestHandler(IEntityContext context,
            ServiceProviderContainer container,
            CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            _container = container;

            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public Task<Response<GetAllResult<TOut>>> HandleAsync(TRequest request, CancellationToken token)
        {
            var provider = _container.CreateProvider();

            ApplyConfiguration(request);

            return HandleWithErrorsAsync(request, provider, token, _HandleAsync);
        }

        private async Task<GetAllResult<TOut>> _HandleAsync(TRequest request, IServiceProvider provider, CancellationToken token)
        {
            var mapper = provider.ProvideInstance<IMapper>();

            await request.RunRequestHooks(RequestConfig, provider, token).Configure();

            var entities = Context.Set<TEntity>()
                .FilterWith(request, RequestConfig, provider)
                .SortWith(request, RequestConfig, provider);
            
            var items = Array.Empty<TOut>();

            if (Options.UseProjection)
            {
                items = await entities.ProjectToArrayAsync<TEntity, TOut>(mapper.ConfigurationProvider, token).Configure();
                token.ThrowIfCancellationRequested();
                
                if (items.Length == 0)
                {
                    if (RequestConfig.ErrorConfig.FailedToFindInGetAllIsError)
                        throw new FailedToFindException { EntityTypeProperty = typeof(TEntity) };

                    var defaultEntity = RequestConfig.GetDefaultFor<TEntity>();
                    if (defaultEntity != null)
                    {
                        items = new TOut[] 
                        {
                            await defaultEntity.CreateResult<TRequest, TEntity, TOut>(request, RequestConfig, provider, token).Configure()
                        };
                    }
                }
            }
            else
            {
                var resultEntities = await entities.ToArrayAsync(token).Configure();
                token.ThrowIfCancellationRequested();

                if (resultEntities.Length == 0)
                {
                    if (RequestConfig.ErrorConfig.FailedToFindInGetAllIsError)
                        throw new FailedToFindException { EntityTypeProperty = typeof(TEntity) };

                    var defaultEntity = RequestConfig.GetDefaultFor<TEntity>();
                    if (defaultEntity != null)
                        resultEntities = new TEntity[] { RequestConfig.GetDefaultFor<TEntity>() };
                }

                await request.RunEntityHooks<TEntity>(RequestConfig, provider, entities, token).Configure();

                items = await resultEntities.CreateResults<TRequest, TEntity, TOut>(request, RequestConfig, provider, token).Configure();
            }

            token.ThrowIfCancellationRequested();

            var result = new GetAllResult<TOut>(items); 

            return await request.RunResultHooks(RequestConfig, provider, result, token).Configure();
        }
    }
}
