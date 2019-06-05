using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Requests
{
    [DoNotValidate]
    public interface ICrudlessRequest
    {
    }

    public interface ICrudlessRequest<TEntity> : ICrudlessRequest
        where TEntity : class
    {
    }

    public interface ICrudlessRequest<TEntity, TOut> : ICrudlessRequest<TEntity>
        where TEntity : class
    {
    }
}
