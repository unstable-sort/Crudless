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
    internal abstract class CreateAllRequestHandlerBase<TRequest, TEntity>
        : CrudlessRequestHandler<TRequest, TEntity>
        where TEntity : class
        where TRequest : ICreateAllRequest, ICrudlessRequest<TEntity>
    {
        protected CreateAllRequestHandlerBase(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        protected async Task<TEntity[]> CreateEntities(TRequest request, IServiceProvider provider, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig, provider, ct).Configure();
            
            var itemSource = RequestConfig.GetRequestItemSourceFor<TEntity>();
            var items = ((IEnumerable<object>)itemSource.ItemSource(request)).ToArray();

            items = await request.RunItemHooks<TEntity>(RequestConfig, provider, items, ct).Configure();
            var entities = await request.CreateEntities<TRequest, TEntity>(RequestConfig, provider, items, ct).Configure();

            await request.RunEntityHooks<TEntity>(RequestConfig, provider, entities, ct).Configure();
            
            entities = await Context.Set<TEntity>().CreateAsync(DataContext, entities, ct).Configure();
            ct.ThrowIfCancellationRequested();
            
            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            await request.RunAuditHooks(RequestConfig, provider, entities.Select(x => ((TEntity)null, x)), ct).Configure();

            return entities;
        }
    }

    internal class CreateAllRequestHandler<TRequest, TEntity>
        : CreateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : ICreateAllRequest<TEntity>, ICrudlessRequest<TEntity>
    {
        private readonly ServiceProviderContainer _container;

        public CreateAllRequestHandler(IEntityContext context, 
            ServiceProviderContainer container,
            CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            _container = container;
        }

        public Task<Response> HandleAsync(TRequest request, CancellationToken token)
        {
            var provider = _container.CreateProvider();

            ApplyConfiguration(request);

            return HandleWithErrorsAsync(request, provider, token,
                (_, p, ct) => (Task)CreateEntities(request, provider, ct));
        }
    }

    internal class CreateAllRequestHandler<TRequest, TEntity, TOut>
        : CreateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, CreateAllResult<TOut>>
        where TEntity : class
        where TRequest : ICreateAllRequest<TEntity, TOut>, ICrudlessRequest<TEntity, TOut>
    {
        private readonly ServiceProviderContainer _container;

        public CreateAllRequestHandler(IEntityContext context,
            ServiceProviderContainer container,
            CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            _container = container;
        }

        public Task<Response<CreateAllResult<TOut>>> HandleAsync(TRequest request, CancellationToken token)
        {
            ApplyConfiguration(request);

            return HandleWithErrorsAsync(request, _container.CreateProvider(), token, _HandleAsync);
        }

        private async Task<CreateAllResult<TOut>> _HandleAsync(TRequest request,
            IServiceProvider provider, 
            CancellationToken token)
        {
            var entities = await CreateEntities(request, provider, token).Configure();
            var items = await entities.CreateResults<TRequest, TEntity, TOut>(request, RequestConfig, provider, token).Configure();
            var result = new CreateAllResult<TOut>(items);

            return await request.RunResultHooks(RequestConfig, provider, result, token).Configure();
        }
    }
}
