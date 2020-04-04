using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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
        
        protected async Task<TEntity> SaveEntity(TRequest request, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig, ct).Configure();

            var item = RequestConfig.GetRequestItemSourceFor<TEntity>().ItemSource(request);

            var entity = await Context.Set<TEntity>()
                .SelectWith(request, RequestConfig)
                .SingleOrDefaultAsync(ct)
                .Configure();

            var oldEntity = Mapper.Map<TEntity, TEntity>(entity);

            ct.ThrowIfCancellationRequested();

            if (entity == null)
            {
                entity = await CreateEntity(request, item, ct).Configure();
                ct.ThrowIfCancellationRequested();
            }
            else
            {
                entity = await UpdateEntity(request, item, entity, ct).Configure();
                ct.ThrowIfCancellationRequested();
            }
            
            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            await request.RunAuditHooks(RequestConfig, oldEntity, entity, ct).Configure();

            return entity;
        }

        private async Task<TEntity> CreateEntity(TRequest request, object item, CancellationToken ct)
        {
            var entity = await request.CreateEntity<TEntity>(RequestConfig, item, ct);

            await request.RunEntityHooks<TEntity>(RequestConfig, entity, ct).Configure();

            entity = await Context.Set<TEntity>().CreateAsync(DataContext, entity, ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entity;
        }

        private async Task<TEntity> UpdateEntity(TRequest request, object item, TEntity entity, CancellationToken ct)
        {
            entity = await request.UpdateEntity(RequestConfig, item, entity, ct).Configure();

            await request.RunEntityHooks<TEntity>(RequestConfig, entity, ct).Configure();

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
        public SaveRequestHandler(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public Task<Response> HandleAsync(TRequest request, CancellationToken token)
        {
            return HandleWithErrorsAsync(request, token, (_, ct) => (Task)SaveEntity(request, ct));
        }
    }

    internal class SaveRequestHandler<TRequest, TEntity, TOut>
        : SaveRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class, new()
        where TRequest : ISaveRequest<TEntity, TOut>, ICrudlessRequest<TEntity, TOut>
    {
        public SaveRequestHandler(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public Task<Response<TOut>> HandleAsync(TRequest request, CancellationToken token)
        {
            return HandleWithErrorsAsync(request, token, _HandleAsync);
        }

        public async Task<TOut> _HandleAsync(TRequest request, CancellationToken token)
        {
            var entity = await SaveEntity(request, token).Configure();
            var tOut = await entity.CreateResult<TEntity, TOut>(RequestConfig, token).Configure();
            var result = await request.RunResultHooks(RequestConfig, tOut, token).Configure();
                
            return result;
        }
    }
}
