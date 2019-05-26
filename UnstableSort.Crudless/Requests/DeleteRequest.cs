using UnstableSort.Crudless.Mediator;
// ReSharper disable UnusedTypeParameter

namespace UnstableSort.Crudless.Requests
{
    public interface IDeleteRequest : ICrudlessRequest
    {
    }

    public interface IDeleteRequest<TEntity> : IDeleteRequest, IRequest
        where TEntity : class
    {       
    }

    public interface IDeleteRequest<TEntity, TOut> : IDeleteRequest, IRequest<TOut>
        where TEntity : class
    {
    }
}
