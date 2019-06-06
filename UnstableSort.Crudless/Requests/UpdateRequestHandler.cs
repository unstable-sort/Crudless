using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Context;
using UnstableSort.Crudless.Exceptions;
using UnstableSort.Crudless.Extensions;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Requests
{
    internal abstract class UpdateRequestHandlerBase<TRequest, TEntity>
        : CrudlessRequestHandler<TRequest, TEntity>
        where TEntity : class, new()
        where TRequest : IUpdateRequest, ICrudlessRequest<TEntity>
    {
        protected UpdateRequestHandlerBase(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
        }
        
        protected async Task<TEntity> UpdateEntity(TRequest request, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig, ct).Configure();

            var item = RequestConfig.GetRequestItemSourceFor<TEntity>().ItemSource(request);

            var entity = await Context.Set<TEntity>()
                .SelectWith(request, RequestConfig)
                .SingleOrDefaultAsync(ct)
                .Configure();
            
            ct.ThrowIfCancellationRequested();

            if (entity != null)
            {
                var oldEntity = Mapper.Map<TEntity, TEntity>(entity);

                entity = await request.UpdateEntity(RequestConfig, item, entity, ct).Configure();

                await request.RunEntityHooks<TEntity>(RequestConfig, entity, ct).Configure();

                entity = await Context.Set<TEntity>().UpdateAsync(DataContext, entity, ct).Configure();
                ct.ThrowIfCancellationRequested();

                await Context.ApplyChangesAsync(ct).Configure();
                ct.ThrowIfCancellationRequested();

                await request.RunAuditHooks(RequestConfig, oldEntity, entity, ct).Configure();
            }
            else if (RequestConfig.ErrorConfig.FailedToFindInUpdateIsError)
            {
                throw new FailedToFindException { EntityTypeProperty = typeof(TEntity) };
            }

            return entity;
        }
    }

    internal class UpdateRequestHandler<TRequest, TEntity>
        : UpdateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class, new()
        where TRequest : IUpdateRequest<TEntity>, ICrudlessRequest<TEntity>
    {
        public UpdateRequestHandler(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public Task<Response> HandleAsync(TRequest request, CancellationToken token)
        {
            return HandleWithErrorsAsync(request, token, (_, ct) => (Task)UpdateEntity(request, ct));
        }
    }

    internal class UpdateRequestHandler<TRequest, TEntity, TOut>
        : UpdateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class, new()
        where TRequest : IUpdateRequest<TEntity, TOut>, ICrudlessRequest<TEntity, TOut>
    {
        public UpdateRequestHandler(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public Task<Response<TOut>> HandleAsync(TRequest request, CancellationToken token)
        {
            return HandleWithErrorsAsync(request, token, _HandleAsync);
        }

        public async Task<TOut> _HandleAsync(TRequest request, CancellationToken token)
        {
            var entity = await UpdateEntity(request, token).Configure();
            var tOut = await entity.CreateResult<TEntity, TOut>(RequestConfig, token).Configure();
            var result = await request.RunResultHooks(RequestConfig, tOut, token).Configure();

            return result;
        }
    }
}
