using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using IServiceProvider = UnstableSort.Crudless.Common.ServiceProvider.IServiceProvider;

namespace UnstableSort.Crudless.Context
{
    public interface IEntitySet<TEntity> : IQueryable<TEntity>, IAsyncEnumerable<TEntity>
    {
    }

    public class EntitySet<TEntity> : IEntitySet<TEntity>
        where TEntity : class
    {
        private readonly ICreateDataAgent _createAgent;
        private readonly IUpdateDataAgent _updateAgent;
        private readonly IDeleteDataAgent _deleteAgent;
        private readonly IBulkCreateDataAgent _bulkCreateAgent;
        private readonly IBulkUpdateDataAgent _bulkUpdateAgent;
        private readonly IBulkDeleteDataAgent _bulkDeleteAgent;

        public EntitySet(IEntitySet<TEntity> entitySetImpl, IServiceProvider provider)
        {
            Implementation = entitySetImpl;

            _createAgent = provider.ProvideInstance<ICreateDataAgent>();
            _updateAgent = provider.ProvideInstance<IUpdateDataAgent>();
            _deleteAgent = provider.ProvideInstance<IDeleteDataAgent>();
            _bulkCreateAgent = provider.ProvideInstance<IBulkCreateDataAgent>();
            _bulkUpdateAgent = provider.ProvideInstance<IBulkUpdateDataAgent>();
            _bulkDeleteAgent = provider.ProvideInstance<IBulkDeleteDataAgent>();
        }

        public IEntitySet<TEntity> Implementation { get; }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => Implementation.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Implementation.GetEnumerator();

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken token = default)
            => Implementation.GetAsyncEnumerator(token);
        
        Type IQueryable.ElementType => Implementation.ElementType;

        Expression IQueryable.Expression => Implementation.Expression;

        IQueryProvider IQueryable.Provider => Implementation.Provider;
        
        public Task<TEntity> CreateAsync(
            DataContext<TEntity> context, 
            TEntity entity,
            CancellationToken token = default(CancellationToken))
            => _createAgent.CreateAsync(context.WithEntitySet(this), entity, token);
        
        public Task<TEntity> UpdateAsync(
            DataContext<TEntity> context,
            TEntity entity, 
            CancellationToken token = default(CancellationToken))
            => _updateAgent.UpdateAsync(context.WithEntitySet(this), entity, token);

        public Task<TEntity> DeleteAsync(
            DataContext<TEntity> context, 
            TEntity entity, 
            CancellationToken token = default(CancellationToken))
            => _deleteAgent.DeleteAsync(context.WithEntitySet(this), entity, token);

        public Task<TEntity[]> CreateAsync(
            DataContext<TEntity> context, 
            IEnumerable<TEntity> entities, 
            CancellationToken token = default(CancellationToken))
            => _bulkCreateAgent.CreateAsync(context.WithEntitySet(this), entities, token);

        public Task<TEntity[]> UpdateAsync(
            DataContext<TEntity> context, 
            IEnumerable<TEntity> entities, 
            CancellationToken token = default(CancellationToken))
            => _bulkUpdateAgent.UpdateAsync(context.WithEntitySet(this), entities, token);

        public Task<TEntity[]> DeleteAsync(
            DataContext<TEntity> context, 
            IEnumerable<TEntity> entities, 
            CancellationToken token = default(CancellationToken))
            => _bulkDeleteAgent.DeleteAsync(context.WithEntitySet(this), entities, token);
    }
}
