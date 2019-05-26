using UnstableSort.Crudless.Mediator;
// ReSharper disable UnusedTypeParameter

namespace UnstableSort.Crudless.Requests
{
    public interface ICreateRequest : ICrudlessRequest
    {
    }

    public interface ICreateRequest<TEntity> : ICreateRequest, IRequest
        where TEntity : class
    {       
    }

    public interface ICreateRequest<TEntity, TOut> : ICreateRequest, IRequest<TOut>
        where TEntity : class
    {
    }
}
