using System.Threading;
using System.Threading.Tasks;

namespace UnstableSort.Crudless
{
    public interface IRequestHook
    {
    }

    public abstract class RequestHook<TRequest> : IRequestHook
    {
        public abstract Task Run(TRequest request, CancellationToken token = default);
    }

    public interface IEntityHook
    {
    }

    public abstract class EntityHook<TRequest, TEntity> : IEntityHook
        where TEntity : class
    {
        public abstract Task Run(TRequest request, TEntity entity, CancellationToken token = default);
    }

    public interface IItemHook
    {
    }

    public abstract class ItemHook<TRequest, TItem> : IItemHook
    {
        public abstract Task<TItem> Run(TRequest request, TItem item, CancellationToken token = default);
    }

    public interface IResultHook
    {
    }

    public abstract class ResultHook<TRequest, TResult> : IResultHook
    {
        public abstract Task<TResult> Run(TRequest request, TResult result, CancellationToken token = default);
    }

    public interface IAuditHook
    {
    }

    public abstract class AuditHook<TRequest, TEntity> : IAuditHook
        where TEntity : class
    {
        public abstract Task Run(TRequest request, TEntity oldEntity, TEntity newEntity, CancellationToken token = default);
    }
}
