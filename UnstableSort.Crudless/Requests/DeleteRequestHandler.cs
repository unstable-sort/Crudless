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
    internal abstract class DeleteRequestHandlerBase<TRequest, TEntity>
        : CrudlessRequestHandler<TRequest, TEntity>
        where TEntity : class, new()
        where TRequest : IDeleteRequest, ICrudlessRequest<TEntity>
    {
        protected DeleteRequestHandlerBase(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
        }
        
        protected async Task<TEntity> DeleteEntity(TRequest request, IServiceProvider provider, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig, provider, ct).Configure();

            var entity = await Context.Set<TEntity>()
                .SelectWith(request, RequestConfig)
                .SingleOrDefaultAsync(ct)
                .Configure();
            
            ct.ThrowIfCancellationRequested();

            if (entity != null)
            {
                var oldEntity = provider.ProvideInstance<IMapper>().Map<TEntity, TEntity>(entity);

                await request.RunEntityHooks<TEntity>(RequestConfig, provider, entity, ct).Configure();

                entity = await Context.Set<TEntity>().DeleteAsync(DataContext, entity, ct).Configure();
                ct.ThrowIfCancellationRequested();

                await Context.ApplyChangesAsync(ct).Configure();
                ct.ThrowIfCancellationRequested();

                await request.RunAuditHooks(RequestConfig, provider, oldEntity, entity, ct).Configure();
            }
            else if (RequestConfig.ErrorConfig.FailedToFindInDeleteIsError)
            {
                throw new FailedToFindException { EntityTypeProperty = typeof(TEntity) };
            }
            
            return entity;
        }
    }

    internal class DeleteRequestHandler<TRequest, TEntity>
        : DeleteRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class, new()
        where TRequest : IDeleteRequest<TEntity>, ICrudlessRequest<TEntity>
    {
        private readonly ServiceProviderContainer _container;

        public DeleteRequestHandler(IEntityContext context,
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
                (_, p, ct) => (Task)DeleteEntity(request, provider, ct));
        }
    }

    internal class DeleteRequestHandler<TRequest, TEntity, TOut>
        : DeleteRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class, new()
        where TRequest : IDeleteRequest<TEntity, TOut>, ICrudlessRequest<TEntity, TOut>
    {
        private readonly ServiceProviderContainer _container;

        public DeleteRequestHandler(IEntityContext context,
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

        public async Task<TOut> _HandleAsync(TRequest request, IServiceProvider provider, CancellationToken token)
        {
            var entity = await DeleteEntity(request, provider, token).Configure();
            var tOut = await entity.CreateResult<TRequest, TEntity, TOut>(request, RequestConfig, provider, token).Configure();
            var result = await request.RunResultHooks(RequestConfig, provider, tOut, token).Configure();

            return result;
        }
    }
}
