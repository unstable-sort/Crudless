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
        private readonly IServiceProvider _provider;

        public EntityFrameworkContext(DbContextFactory contextFactory,
            ServiceProviderContainer container)
        {
            _contextFactory = contextFactory;
            _provider = container.CreateProvider();
        }

        protected DbContext DbContext { get; private set; }
        
        public bool HasTransaction => DbContext?.Database.CurrentTransaction != null;

        public virtual EntitySet<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (DbContext == null)
                DbContext = _contextFactory.FromEntityType<TEntity>(_provider);

            return new EntitySet<TEntity>(new EntityFrameworkEntitySet<TEntity>(DbContext), _provider);
        }

        public virtual Task<IEntityContextTransaction> BeginTransactionAsync<TRequest>(CancellationToken token = default(CancellationToken))
        {
            if (DbContext == null)
                DbContext = _contextFactory.FromRequestType<TRequest>(_provider);

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
