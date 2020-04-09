using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Context;

using IServiceProvider = UnstableSort.Crudless.Common.ServiceProvider.IServiceProvider;

namespace UnstableSort.Crudless.Tests.ContextTests
{
    public class InMemoryContext : IEntityContext
    {
        private readonly IServiceProvider _provider;

        private static readonly Dictionary<Type, Tuple<IInMemorySet, IList>> _sets
            = new Dictionary<Type, Tuple<IInMemorySet, IList>>();

        public InMemoryContext(ServiceProviderContainer container)
        {
            _provider = container.GetProvider();
        }

        public bool HasTransaction => true;

        internal static void Clear()
        {
            _sets.Clear();
        }

        public Task<int> ApplyChangesAsync(CancellationToken token = default(CancellationToken))
            => Task.FromResult(0);

        public Task<IEntityContextTransaction> BeginTransactionAsync<TRequest>(CancellationToken token = default(CancellationToken))
            => Task.FromResult((IEntityContextTransaction)new NullTransaction());

        public EntitySet<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (!_sets.TryGetValue(typeof(TEntity), out var _))
            {
                var dataList = new List<TEntity>();

                _sets[typeof(TEntity)] = 
                    Tuple.Create<IInMemorySet, IList>(new InMemorySet<TEntity>(dataList), dataList);
            }

            return new EntitySet<TEntity>(_sets[typeof(TEntity)].Item1 as InMemorySet<TEntity>, _provider);
        }
    }

    public class NullTransaction : IEntityContextTransaction
    {
        public Guid TransactionId => Guid.Empty;

        public void Commit() { }
        
        public void Rollback() { }

        public void Dispose() { }
    }
}
