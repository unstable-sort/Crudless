using System.Threading;
using System.Threading.Tasks;

namespace UnstableSort.Crudless.Context
{
    public interface IEntityContext
    {
        EntitySet<TEntity> Set<TEntity>() where TEntity : class;

        Task<int> ApplyChangesAsync(CancellationToken token = default(CancellationToken));
    }
}