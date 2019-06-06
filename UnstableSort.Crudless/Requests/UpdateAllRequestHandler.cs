using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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
        
        protected async Task<TEntity[]> UpdateEntities(TRequest request, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig, ct).Configure();

            var itemSource = RequestConfig.GetRequestItemSourceFor<TEntity>();
            var items = ((IEnumerable<object>)itemSource.ItemSource(request)).ToArray();

            items = await request.RunItemHooks<TEntity>(RequestConfig, items, ct).Configure();

            var entities = await Context.Set<TEntity>()
                .FilterWith(request, RequestConfig)
                .SelectWith(request, RequestConfig)
                .ToArrayAsync(ct)
                .Configure();

            ct.ThrowIfCancellationRequested();

            var auditEntities = entities
                .Select(x => (Mapper.Map<TEntity, TEntity>(x), x))
                .ToArray();

            var joinedItems = RequestConfig.Join(items, entities).Where(x => x.Item2 != null);

            var updatedEntities = await request.UpdateEntities(RequestConfig, joinedItems, ct).Configure();

            await request.RunEntityHooks<TEntity>(RequestConfig, entities, ct).Configure();
            
            entities = await Context.Set<TEntity>().UpdateAsync(DataContext, updatedEntities, ct).Configure();
            ct.ThrowIfCancellationRequested();

            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            await request.RunAuditHooks(RequestConfig, auditEntities, ct).Configure();

            return entities;
        }
    }

    internal class UpdateAllRequestHandler<TRequest, TEntity>
        : UpdateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class, new()
        where TRequest : IUpdateAllRequest<TEntity>, ICrudlessRequest<TEntity>
    {
        public UpdateAllRequestHandler(IEntityContext context,
            CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public Task<Response> HandleAsync(TRequest request, CancellationToken token)
        {
            return HandleWithErrorsAsync(request, token, (_, ct) => (Task)UpdateEntities(request, ct));
        }
    }

    internal class UpdateAllRequestHandler<TRequest, TEntity, TOut>
        : UpdateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, UpdateAllResult<TOut>>
        where TEntity : class, new()
        where TRequest : IUpdateAllRequest<TEntity, TOut>, ICrudlessRequest<TEntity, TOut>
    {
        public UpdateAllRequestHandler(IEntityContext context,
            CrudlessConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public Task<Response<UpdateAllResult<TOut>>> HandleAsync(TRequest request, CancellationToken token)
        {
            return HandleWithErrorsAsync(request, token, _HandleAsync);
        }

        public async Task<UpdateAllResult<TOut>> _HandleAsync(TRequest request, CancellationToken token)
        {
            var entities = await UpdateEntities(request, token).Configure();
            var items = await entities.CreateResults<TEntity, TOut>(RequestConfig, token).Configure();
            var result = new UpdateAllResult<TOut>(items);

            return await request.RunResultHooks(RequestConfig, result, token).Configure();
        }
    }
}
