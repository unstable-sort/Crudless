namespace UnstableSort.Crudless.Requests
{
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
