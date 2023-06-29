using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Context;
using UnstableSort.Crudless.Extensions;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Requests
{
    internal abstract class SaveRequestHandlerBase<TRequest, TEntity>
        : CrudlessRequestHandler<TRequest, TEntity>
        where TEntity : class, new()
        where TRequest : ISaveRequest, ICrudlessRequest<TEntity>
    {
        protected readonly RequestOptions Options;

        protected SaveRequestHandlerBase(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }
        
        protected async Task<TEntity> SaveEntity(TRequest request, IServiceProvider provider, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig, provider, ct).Configure();

            var item = RequestConfig.GetRequestItemSourceFor<TEntity>().ItemSource(request);

            var entity = await Context.Set<TEntity>()
                .SelectWith(request, RequestConfig)
                .SingleOrDefaultAsync(ct)
                .Configure();

            var oldEntity = provider.ProvideInstance<IObjectMapper>().Clone(entity);

            ct.ThrowIfCancellationRequested();

            if (entity == null)
            {
                entity = await CreateEntity(request, provider, item, ct).Configure();
                ct.ThrowIfCancellationRequested();
            }
            else
            {
                entity = await UpdateEntity(request, provider, item, entity, ct).Configure();
                ct.ThrowIfCancellationRequested();
            }
            
            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            await request.RunAuditHooks(RequestConfig, provider, oldEntity, entity, ct).Configure();

            return entity;
        }

        private async Task<TEntity> CreateEntity(TRequest request, IServiceProvider provider, object item, CancellationToken ct)
        {
            var entity = await request.CreateEntity<TRequest, TEntity>(RequestConfig, provider, item, ct);

            await request.RunEntityHooks<TEntity>(RequestConfig, provider, entity, ct).Configure();

            entity = await Context.Set<TEntity>().CreateAsync(DataContext, entity, ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entity;
        }

        private async Task<TEntity> UpdateEntity(TRequest request, IServiceProvider provider, object item, TEntity entity, CancellationToken ct)
        {
            entity = await request.UpdateEntity(RequestConfig, provider, item, entity, ct).Configure();

            await request.RunEntityHooks<TEntity>(RequestConfig, provider, entity, ct).Configure();

            entity = await Context.Set<TEntity>().UpdateAsync(DataContext, entity, ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entity;
        }
    }

    internal class SaveRequestHandler<TRequest, TEntity>
        : SaveRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class, new()
        where TRequest : ISaveRequest<TEntity>, ICrudlessRequest<TEntity>
    {
        private readonly ServiceProviderContainer _container;

        public SaveRequestHandler(IEntityContext context,
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
                (_, p, ct) => (Task)SaveEntity(request, provider, ct));
        }
    }

    internal class SaveRequestHandler<TRequest, TEntity, TOut>
        : SaveRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class, new()
        where TRequest : ISaveRequest<TEntity, TOut>, ICrudlessRequest<TEntity, TOut>
    {
        private readonly ServiceProviderContainer _container;

        public SaveRequestHandler(IEntityContext context,
            ServiceProviderContainer container,
            CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
            _container = container;
        }

        public Task<Response<TOut>> HandleAsync(TRequest request, CancellationToken token)
        {
            var provider = _container.GetProvider();

            ApplyConfiguration(request);

            return HandleWithErrorsAsync(request, provider, token, _HandleAsync);
        }

        public async Task<TOut> _HandleAsync(TRequest request,
            IServiceProvider provider,
            CancellationToken token)
        {
            var entity = await SaveEntity(request, provider, token).Configure();
            var tOut = await entity.CreateResult<TRequest, TEntity, TOut>(request, RequestConfig, provider, token).Configure();
            var result = await request.RunResultHooks(RequestConfig, provider, tOut, token).Configure();
                
            return result;
        }
    }
}
