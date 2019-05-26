using UnstableSort.Crudless.Mediator;
// ReSharper disable UnusedTypeParameter

namespace UnstableSort.Crudless.Requests
{
    public interface ISaveRequest : ICrudlessRequest
    {
    }

    public interface ISaveRequest<TEntity> : ISaveRequest, IRequest
        where TEntity : class
    {
    }

    public interface ISaveRequest<TEntity, TOut> : ISaveRequest, IRequest<TOut>
        where TEntity : class
    {
    }
}
