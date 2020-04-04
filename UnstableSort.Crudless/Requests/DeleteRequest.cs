using UnstableSort.Crudless.Mediator;
// ReSharper disable UnusedTypeParameter

namespace UnstableSort.Crudless.Requests
{
    public interface IDeleteRequest
    {
    }

    public interface IDeleteRequest<TEntity> 
        : IDeleteRequest, IRequest, IAuditedRequest<TEntity>, ICrudlessRequest<TEntity>
        where TEntity : class
    {       
    }

    public interface IDeleteRequest<TEntity, TOut> 
        : IDeleteRequest, IRequest<TOut>, IAuditedRequest<TEntity>, ICrudlessRequest<TEntity, TOut>
        where TEntity : class
    {
    }
}
