using UnstableSort.Crudless.Mediator;
// ReSharper disable UnusedTypeParameter

namespace UnstableSort.Crudless.Requests
{
    public interface ISaveRequest
    {
    }

    public interface ISaveRequest<TEntity> 
        : ISaveRequest, IRequest, IAuditedRequest<TEntity>, ICrudlessRequest<TEntity>
        where TEntity : class
    {
    }

    public interface ISaveRequest<TEntity, TOut> 
        : ISaveRequest, IRequest<TOut>, IAuditedRequest<TEntity>, ICrudlessRequest<TEntity, TOut>
        where TEntity : class
    {
    }
}
