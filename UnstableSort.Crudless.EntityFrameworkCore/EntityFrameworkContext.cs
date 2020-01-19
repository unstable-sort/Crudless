using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UnstableSort.Crudless.Context;

namespace UnstableSort.Crudless.EntityFrameworkCore
{
    public class EntityFrameworkContext : IEntityContext
    {
        private readonly DbContextFactory _contextFactory;

        public EntityFrameworkContext(DbContextFactory contextFactory, 
            IDataAgentFactory dataAgentFactory)
        {
            _contextFactory = contextFactory;
            DataAgentFactory = dataAgentFactory;
        }

        protected DbContext DbContext { get; private set; }

        protected IDataAgentFactory DataAgentFactory { get; }

        public bool HasTransaction => DbContext?.Database.CurrentTransaction != null;

        public virtual EntitySet<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (DbContext == null)
                DbContext = _contextFactory.FromEntityType<TEntity>();

            return new EntityFrameworkEntitySet<TEntity>(DbContext, DataAgentFactory);
        }

        public virtual Task<IEntityContextTransaction> BeginTransactionAsync<TRequest>(CancellationToken token = default(CancellationToken))
        {
            if (DbContext == null)
                DbContext = _contextFactory.FromRequestType<TRequest>();

            return EntityFrameworkContextTransaction.BeginAsync(DbContext, token);
        }

        public virtual async Task<int> ApplyChangesAsync(CancellationToken token = default(CancellationToken))
        {
            if (DbContext == null)
                return 0;

            var result = await DbContext.SaveChangesAsync(token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            return result;
        }
    }
}
