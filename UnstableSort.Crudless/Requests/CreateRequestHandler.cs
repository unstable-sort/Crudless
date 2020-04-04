using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Context;
using UnstableSort.Crudless.Extensions;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Requests
{
    internal abstract class CreateRequestHandlerBase<TRequest, TEntity>
        : CrudlessRequestHandler<TRequest, TEntity>
        where TEntity : class
        where TRequest : ICreateRequest, ICrudlessRequest
    {
        protected CreateRequestHandlerBase(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        protected async Task<TEntity> CreateEntity(TRequest request,
            IServiceProvider provider, 
            CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig, provider, ct).Configure();
            
            var item = RequestConfig.GetRequestItemSourceFor<TEntity>().ItemSource(request);
            var entity = await request.CreateEntity<TRequest, TEntity>(RequestConfig, provider, item, ct).Configure();

            await request.RunEntityHooks<TEntity>(RequestConfig, provider, entity, ct).Configure();

            entity = await Context.Set<TEntity>().CreateAsync(DataContext, entity, ct).Configure();
            ct.ThrowIfCancellationRequested();
            
            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            await request.RunAuditHooks(RequestConfig, provider, null, entity, ct).Configure();

            return entity;
        }
    }

    internal class CreateRequestHandler<TRequest, TEntity>
        : CreateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : ICreateRequest<TEntity>, ICrudlessRequest<TEntity>
    {
        private readonly ServiceProviderContainer _container;

        public CreateRequestHandler(IEntityContext context,
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
                (_, p, ct) => (Task)CreateEntity(request, provider, ct));
        }
    }

    internal class CreateRequestHandler<TRequest, TEntity, TOut>
        : CreateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : ICreateRequest<TEntity, TOut>, ICrudlessRequest<TEntity, TOut>
    {
        private readonly ServiceProviderContainer _container;

        public CreateRequestHandler(IEntityContext context, 
            ServiceProviderContainer container,
            CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            _container = container;
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
            var entity = await CreateEntity(request, provider, token).Configure();
            var tOut = await entity.CreateResult<TRequest, TEntity, TOut>(request, RequestConfig, provider, token).Configure();
            var result = await request.RunResultHooks(RequestConfig, provider, tOut, token).Configure();

            return result;
        }
    }
}
