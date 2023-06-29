using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Context;
using UnstableSort.Crudless.Exceptions;
using UnstableSort.Crudless.Extensions;
using UnstableSort.Crudless.Mediator;
using IServiceProvider = UnstableSort.Crudless.Common.ServiceProvider.IServiceProvider;

namespace UnstableSort.Crudless.Requests
{
    internal class PagedGetAllRequestHandler<TRequest, TEntity, TOut>
        : CrudlessRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, PagedGetAllResult<TOut>>
        where TEntity : class
        where TRequest : IPagedGetAllRequest<TEntity, TOut>, ICrudlessRequest<TEntity, TOut>
    {
        private readonly ServiceProviderContainer _container;

        protected readonly RequestOptions Options;

        public PagedGetAllRequestHandler(IEntityContext context,
            ServiceProviderContainer container, 
            CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            _container = container;

            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public Task<Response<PagedGetAllResult<TOut>>> HandleAsync(TRequest request, CancellationToken token)
        {
            var provider = _container.GetProvider();

            ApplyConfiguration(request);

            return HandleWithErrorsAsync(request, provider, token, _HandleAsync);
        }

        public async Task<PagedGetAllResult<TOut>> _HandleAsync(TRequest request, 
            IServiceProvider provider, 
            CancellationToken token)
        {
            await request.RunRequestHooks(RequestConfig, provider, token).Configure();
                    
            var entities = Context
                .Set<TEntity>()
                .FilterWith(request, RequestConfig, provider)
                .SortWith(request, RequestConfig, provider);
                    
            var totalItemCount = await entities.CountAsync(token).Configure();
            token.ThrowIfCancellationRequested();

            var pageSize = request.PageSize < 1 ? totalItemCount : request.PageSize;
            var totalPageCount = totalItemCount == 0 ? 1 : (totalItemCount + pageSize - 1) / pageSize;
            var pageNumber = Math.Max(1, Math.Min(request.PageNumber, totalPageCount));
            var startIndex = (pageNumber - 1) * pageSize;

            PagedGetAllResult<TOut> result;

            if (totalItemCount != 0)
            {
                entities = entities.Skip(startIndex).Take(pageSize);
                var items = await GetItems(request, entities, provider, token).Configure();
                result = new PagedGetAllResult<TOut>(items, pageNumber, pageSize, totalPageCount, totalItemCount);
            }
            else
            {
                result = new PagedGetAllResult<TOut>(Array.Empty<TOut>(), 1, pageSize, 1, 0);
            }

            return await request.RunResultHooks(RequestConfig, provider, result, token).Configure();
        }

        private async Task<TOut[]> GetItems(TRequest request, 
            IQueryable<TEntity> entities, 
            IServiceProvider provider,
            CancellationToken token)
        {
            var mapper = provider.ProvideInstance<IObjectMapper>();
            var items = Array.Empty<TOut>();

            if (Options.UseProjection)
            {
                items = await entities.ProjectToArrayAsync<TEntity, TOut>(mapper, token).Configure();
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

            return items;
        }
    }
}
