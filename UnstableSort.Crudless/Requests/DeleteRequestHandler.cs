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
    internal abstract class DeleteRequestHandlerBase<TRequest, TEntity>
        : CrudlessRequestHandler<TRequest, TEntity>
        where TEntity : class, new()
        where TRequest : IDeleteRequest, ICrudlessRequest<TEntity>
    {
        protected DeleteRequestHandlerBase(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
        }
        
        protected async Task<TEntity> DeleteEntity(TRequest request, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig, ct).Configure();

            var entity = await Context.Set<TEntity>()
                .SelectWith(request, RequestConfig)
                .SingleOrDefaultAsync(ct)
                .Configure();


            ct.ThrowIfCancellationRequested();

            if (entity != null)
            {
                var oldEntity = Mapper.Map<TEntity, TEntity>(entity);

                await request.RunEntityHooks<TEntity>(RequestConfig, entity, ct).Configure();

                entity = await Context.Set<TEntity>().DeleteAsync(DataContext, entity, ct).Configure();
                ct.ThrowIfCancellationRequested();

                await Context.ApplyChangesAsync(ct).Configure();
                ct.ThrowIfCancellationRequested();

                await request.RunAuditHooks(RequestConfig, oldEntity, entity, ct).Configure();
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
        public DeleteRequestHandler(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public Task<Response> HandleAsync(TRequest request, CancellationToken token)
        {
            return HandleWithErrorsAsync(request, token, (_, ct) => (Task)DeleteEntity(request, ct));
        }
    }

    internal class DeleteRequestHandler<TRequest, TEntity, TOut>
        : DeleteRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class, new()
        where TRequest : IDeleteRequest<TEntity, TOut>, ICrudlessRequest<TEntity, TOut>
    {
        public DeleteRequestHandler(IEntityContext context, CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public Task<Response<TOut>> HandleAsync(TRequest request, CancellationToken token)
        {
            return HandleWithErrorsAsync(request, token, _HandleAsync);
        }

        public async Task<TOut> _HandleAsync(TRequest request, CancellationToken token)
        {
            var entity = await DeleteEntity(request, token).Configure();
            var tOut = await entity.CreateResult<TEntity, TOut>(RequestConfig, token).Configure();
            var result = await request.RunResultHooks(RequestConfig, tOut, token).Configure();

            return result;
        }
    }
}
