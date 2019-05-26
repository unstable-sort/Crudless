using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UnstableSort.Crudless.Context;

namespace UnstableSort.Crudless.EntityFrameworkCore
{
    public class EntityFrameworkContext : IEntityContext
    {
        protected DbContext DbContext { get; }

        protected IDataAgentFactory DataAgentFactory { get; }

        public EntityFrameworkContext(DbContext context, 
            IDataAgentFactory dataAgentFactory)
        {
            DbContext = context;
            DataAgentFactory = dataAgentFactory;
        }

        public virtual EntitySet<TEntity> Set<TEntity>() 
            where TEntity : class
            => new EntityFrameworkEntitySet<TEntity>(DbContext, DataAgentFactory);

        public virtual async Task<int> ApplyChangesAsync(CancellationToken token = default(CancellationToken))
        {
            var result = await DbContext.SaveChangesAsync(token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            return result;
        }
    }
}
