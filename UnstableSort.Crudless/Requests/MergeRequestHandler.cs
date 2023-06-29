using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Context;
using UnstableSort.Crudless.Extensions;
using UnstableSort.Crudless.Mediator;

using IServiceProvider = UnstableSort.Crudless.Common.ServiceProvider.IServiceProvider;

namespace UnstableSort.Crudless.Requests
{
    internal abstract class MergeRequestHandlerBase<TRequest, TEntity>
        : CrudlessRequestHandler<TRequest, TEntity>
        where TEntity : class, new()
        where TRequest : IMergeRequest, ICrudlessRequest<TEntity>
    {
        protected readonly RequestOptions Options;

        protected MergeRequestHandlerBase(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        protected async Task<TEntity[]> MergeEntities(TRequest request, IServiceProvider provider, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig, provider, ct).Configure();

            var itemSource = RequestConfig.GetRequestItemSourceFor<TEntity>();
            var items = ((IEnumerable<object>)itemSource.ItemSource(request)).ToArray();

            items = await request.RunItemHooks<TEntity>(RequestConfig, provider, items, ct).Configure();

            var entities = await Context.Set<TEntity>()
                .FilterWith(request, RequestConfig, provider)
                .SelectWith(request, RequestConfig)
                .ToArrayAsync(ct)
                .Configure();

            var auditEntities = entities
                .Select(x => (provider.ProvideInstance<IObjectMapper>().Clone(x), x))
                .ToArray();

            ct.ThrowIfCancellationRequested();

            var joinedItems = RequestConfig
                .Join(items.Where(x => x != null).ToArray(), entities)
                .ToArray();

            var createdEntities = await CreateEntities(request, 
                provider,
                joinedItems.Where(x => x.Item2 == null).Select(x => x.Item1), ct).Configure();

            ct.ThrowIfCancellationRequested();

            var updatedEntities = await UpdateEntities(request, 
                provider,
                joinedItems.Where(x => x.Item2 != null), ct).Configure();

            ct.ThrowIfCancellationRequested();

            var mergedEntities = updatedEntities.Concat(createdEntities).ToArray();
            
            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            await request.RunAuditHooks(RequestConfig, provider, auditEntities, ct).Configure();

            return mergedEntities;
        }

        private async Task<TEntity[]> CreateEntities(TRequest request, 
            IServiceProvider provider, 
            IEnumerable<object> items, 
            CancellationToken ct)
        {
            var entities = await request.CreateEntities<TRequest, TEntity>(RequestConfig, provider, items, ct).Configure();

            await request.RunEntityHooks<TEntity>(RequestConfig, provider, entities, ct).Configure();

            entities = await Context.Set<TEntity>().CreateAsync(DataContext, entities, ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entities;
        }

        private async Task<TEntity[]> UpdateEntities(TRequest request, 
            IServiceProvider provider, 
            IEnumerable<Tuple<object, TEntity>> items,
            CancellationToken ct)
        {
            var entities = await request.UpdateEntities(RequestConfig, provider, items, ct).Configure();

            await request.RunEntityHooks<TEntity>(RequestConfig, provider, entities, ct).Configure();

            entities = await Context.Set<TEntity>().UpdateAsync(DataContext, entities, ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entities;
        }
    }

    internal class MergeRequestHandler<TRequest, TEntity>
        : MergeRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class, new()
        where TRequest : IMergeRequest<TEntity>, ICrudlessRequest<TEntity>
    {
        private readonly ServiceProviderContainer _container;

        public MergeRequestHandler(IEntityContext context,
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
                (_, p, ct) => (Task)MergeEntities(request, provider, ct));
        }
    }

    internal class MergeRequestHandler<TRequest, TEntity, TOut>
        : MergeRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, MergeResult<TOut>>
        where TEntity : class, new()
        where TRequest : IMergeRequest<TEntity, TOut>, ICrudlessRequest<TEntity, TOut>
    {
        private readonly ServiceProviderContainer _container;

        public MergeRequestHandler(IEntityContext context,
            ServiceProviderContainer container,
            CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            _container = container;
        }

        public Task<Response<MergeResult<TOut>>> HandleAsync(TRequest request, CancellationToken token)
        {
            var provider = _container.GetProvider();

            ApplyConfiguration(request);

            return HandleWithErrorsAsync(request, provider, token, _HandleAsync);
        }

        public async Task<MergeResult<TOut>> _HandleAsync(TRequest request,
            IServiceProvider provider, 
            CancellationToken token)
        {
            var entities = await MergeEntities(request, provider, token).Configure();
            var items = await entities.CreateResults<TRequest, TEntity, TOut>(request, RequestConfig, provider, token).Configure();
            var result = new MergeResult<TOut>(items);

            return await request.RunResultHooks(RequestConfig, provider, result, token).Configure();
        }
    }
}
