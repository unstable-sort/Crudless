using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Context;
using UnstableSort.Crudless.Extensions;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Requests
{
    internal abstract class UpdateAllRequestHandlerBase<TRequest, TEntity>
        : CrudlessRequestHandler<TRequest, TEntity>
        where TEntity : class, new()
        where TRequest : IUpdateAllRequest, ICrudlessRequest<TEntity>
    {
        protected readonly RequestOptions Options;

        protected UpdateAllRequestHandlerBase(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }
        
        protected async Task<TEntity[]> UpdateEntities(TRequest request, IServiceProvider provider, CancellationToken ct)
        {
            var mapper = provider.ProvideInstance<IObjectMapper>();

            await request.RunRequestHooks(RequestConfig, provider, ct).Configure();

            var itemSource = RequestConfig.GetRequestItemSourceFor<TEntity>();
            var items = ((IEnumerable<object>)itemSource.ItemSource(request)).ToArray();

            items = await request.RunItemHooks<TEntity>(RequestConfig, provider, items, ct).Configure();

            var entities = await Context.Set<TEntity>()
                .FilterWith(request, RequestConfig, provider)
                .SelectWith(request, RequestConfig)
                .ToArrayAsync(ct)
                .Configure();

            ct.ThrowIfCancellationRequested();

            var auditEntities = entities
                .Select(x => (mapper.Clone(x), x))
                .ToArray();

            var joinedItems = RequestConfig.Join(items, entities).Where(x => x.Item2 != null);

            var updatedEntities = await request.UpdateEntities(RequestConfig, provider, joinedItems, ct).Configure();

            await request.RunEntityHooks<TEntity>(RequestConfig, provider, entities, ct).Configure();
            
            entities = await Context.Set<TEntity>().UpdateAsync(DataContext, updatedEntities, ct).Configure();
            ct.ThrowIfCancellationRequested();

            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            await request.RunAuditHooks(RequestConfig, provider, auditEntities, ct).Configure();

            return entities;
        }
    }

    internal class UpdateAllRequestHandler<TRequest, TEntity>
        : UpdateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class, new()
        where TRequest : IUpdateAllRequest<TEntity>, ICrudlessRequest<TEntity>
    {
        private readonly ServiceProviderContainer _container;

        public UpdateAllRequestHandler(IEntityContext context,
            ServiceProviderContainer container,
            CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            _container = container;
        }

        public Task<Response> HandleAsync(TRequest request, CancellationToken token)
        {
            var provider = _container.GetProvider();

            ApplyConfiguration(request);

            return HandleWithErrorsAsync(request, provider, token,
                (_, p, ct) => (Task)UpdateEntities(request, provider, ct));
        }
    }

    internal class UpdateAllRequestHandler<TRequest, TEntity, TOut>
        : UpdateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, UpdateAllResult<TOut>>
        where TEntity : class, new()
        where TRequest : IUpdateAllRequest<TEntity, TOut>, ICrudlessRequest<TEntity, TOut>
    {
        private readonly ServiceProviderContainer _container;

        public UpdateAllRequestHandler(IEntityContext context,
            ServiceProviderContainer container,
            CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            _container = container;
        }

        public Task<Response<UpdateAllResult<TOut>>> HandleAsync(TRequest request, CancellationToken token)
        {
            var provider = _container.GetProvider();

            ApplyConfiguration(request);

            return HandleWithErrorsAsync(request, provider, token, _HandleAsync);
        }

        public async Task<UpdateAllResult<TOut>> _HandleAsync(TRequest request, 
            IServiceProvider provider,
            CancellationToken token)
        {
            var entities = await UpdateEntities(request, provider, token).Configure();
            var items = await entities.CreateResults<TRequest, TEntity, TOut>(request, RequestConfig, provider, token).Configure();
            var result = new UpdateAllResult<TOut>(items);

            return await request.RunResultHooks(RequestConfig, provider, result, token).Configure();
        }
    }
}
