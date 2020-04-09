using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Context;

namespace UnstableSort.Crudless.Integration.EntityFrameworkCore
{
    public class EntityFrameworkContext : IEntityContext
    {
        private readonly DbContextFactory _contextFactory;
        private readonly ServiceProviderContainer _provider;

        public EntityFrameworkContext(DbContextFactory contextFactory,
            ServiceProviderContainer provider)
        {
            _contextFactory = contextFactory;
            _provider = provider;
        }

        protected DbContext DbContext { get; private set; }
        
        public bool HasTransaction => DbContext?.Database.CurrentTransaction != null;

        public virtual EntitySet<TEntity> Set<TEntity>()
            where TEntity : class
        {
            var provider = _provider.GetProvider();

            if (DbContext == null)
                DbContext = _contextFactory.FromEntityType<TEntity>(provider);

            return new EntitySet<TEntity>(new EntityFrameworkEntitySet<TEntity>(DbContext), provider);
        }

        public virtual Task<IEntityContextTransaction> BeginTransactionAsync<TRequest>(CancellationToken token = default(CancellationToken))
        {
            if (DbContext == null)
                DbContext = _contextFactory.FromRequestType<TRequest>(_provider.GetProvider());

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
