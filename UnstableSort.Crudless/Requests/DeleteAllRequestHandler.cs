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
    internal abstract class DeleteAllRequestHandlerBase<TRequest, TEntity>
        : CrudlessRequestHandler<TRequest, TEntity>
        where TEntity : class, new()
        where TRequest : IDeleteAllRequest, ICrudlessRequest<TEntity>
    {
        protected readonly RequestOptions Options;

        protected DeleteAllRequestHandlerBase(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        protected async Task<TEntity[]> DeleteEntities(TRequest request, IServiceProvider provider, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig, provider, ct).Configure();

            ct.ThrowIfCancellationRequested();

            var entities = await Context.Set<TEntity>()
                .FilterWith(request, RequestConfig, provider)
                .ToArrayAsync(ct)
                .Configure();

            var mapper = provider.ProvideInstance<IObjectMapper>();

            var auditEntities = entities
                .Select(x => (mapper.Clone(x), x))
                .ToArray();

            ct.ThrowIfCancellationRequested();

            await request.RunEntityHooks<TEntity>(RequestConfig, provider, entities, ct).Configure();
            
            entities = await Context.Set<TEntity>().DeleteAsync(DataContext, entities, ct).Configure();
            ct.ThrowIfCancellationRequested();

            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            await request.RunAuditHooks(RequestConfig, provider, auditEntities, ct).Configure();

            return entities;
        }
    }

    internal class DeleteAllRequestHandler<TRequest, TEntity>
        : DeleteAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class, new()
        where TRequest : IDeleteAllRequest<TEntity>, ICrudlessRequest<TEntity>
    {
        private readonly ServiceProviderContainer _container;

        public DeleteAllRequestHandler(IEntityContext context,
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
                (_, p, ct) => (Task)DeleteEntities(request, provider, ct));    
        }
    }

    internal class DeleteAllRequestHandler<TRequest, TEntity, TOut>
        : DeleteAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, DeleteAllResult<TOut>>
        where TEntity : class, new()
        where TRequest : IDeleteAllRequest<TEntity, TOut>, ICrudlessRequest<TEntity, TOut>
    {
        private readonly ServiceProviderContainer _container;

        public DeleteAllRequestHandler(IEntityContext context,
            ServiceProviderContainer container, 
            CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            _container = container;
        }

        public Task<Response<DeleteAllResult<TOut>>> HandleAsync(TRequest request, CancellationToken token)
        {
            var provider = _container.GetProvider();

            ApplyConfiguration(request);

            return HandleWithErrorsAsync(request, provider, token, _HandleAsync);
        }

        private async Task<DeleteAllResult<TOut>> _HandleAsync(TRequest request,
            IServiceProvider provider, 
            CancellationToken token)
        {
            var entities = await DeleteEntities(request, provider, token).Configure();
            var items = await entities.CreateResults<TRequest, TEntity, TOut>(request, RequestConfig, provider, token).Configure();
            var result = new DeleteAllResult<TOut>(items);

            return await request.RunResultHooks(RequestConfig, provider, result, token).Configure();
        }
    }
}
