using System.Threading;
using System.Threading.Tasks;

namespace UnstableSort.Crudless.Context
{
    public interface IEntityContext
    {
        bool HasTransaction { get; }
        
        EntitySet<TEntity> Set<TEntity>() 
            where TEntity : class;

        Task<IEntityContextTransaction> BeginTransactionAsync<TRequest>(CancellationToken token = default(CancellationToken));

        Task<int> ApplyChangesAsync(CancellationToken token = default(CancellationToken));
    }
}