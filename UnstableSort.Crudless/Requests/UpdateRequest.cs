using UnstableSort.Crudless.Mediator;
// ReSharper disable UnusedTypeParameter

namespace UnstableSort.Crudless.Requests
{
    public interface IUpdateRequest
    {
    }

    public interface IUpdateRequest<TEntity> 
        : IUpdateRequest, IRequest, IAuditedRequest<TEntity>, ICrudlessRequest<TEntity>
        where TEntity : class
    {
    }

    public interface IUpdateRequest<TEntity, TOut> 
        : IUpdateRequest, IRequest<TOut>, IAuditedRequest<TEntity>, ICrudlessRequest<TEntity, TOut>
        where TEntity : class
    {
    }
}
