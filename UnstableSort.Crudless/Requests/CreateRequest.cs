using UnstableSort.Crudless.Mediator;
// ReSharper disable UnusedTypeParameter

namespace UnstableSort.Crudless.Requests
{
    public interface ICreateRequest
    {
    }

    public interface ICreateRequest<TEntity> 
        : ICreateRequest, IRequest, IAuditedRequest<TEntity>, ICrudlessRequest<TEntity>
        where TEntity : class
    {       
    }

    public interface ICreateRequest<TEntity, TOut> 
        : ICreateRequest, IRequest<TOut>, IAuditedRequest<TEntity>, ICrudlessRequest<TEntity, TOut>
        where TEntity : class
    {
    }
}
