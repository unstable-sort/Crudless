using UnstableSort.Crudless.Mediator;
// ReSharper disable UnusedTypeParameter

namespace UnstableSort.Crudless.Requests
{
    public interface IUpdateRequest : ICrudlessRequest
    {
    }

    public interface IUpdateRequest<TEntity> : IUpdateRequest, IRequest
        where TEntity : class
    {
    }

    public interface IUpdateRequest<TEntity, TOut> : IUpdateRequest, IRequest<TOut>
        where TEntity : class
    {
    }
}
