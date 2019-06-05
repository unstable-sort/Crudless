using UnstableSort.Crudless.Mediator;
// ReSharper disable UnusedTypeParameter

namespace UnstableSort.Crudless.Requests
{
    public interface IGetRequest
    {
    }

    public interface IGetRequest<TEntity, TOut> 
        : IGetRequest, IRequest<TOut>, ICrudlessRequest<TEntity, TOut>
        where TEntity : class
    {
    }
}
