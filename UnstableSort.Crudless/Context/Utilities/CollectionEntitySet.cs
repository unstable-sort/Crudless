using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace UnstableSort.Crudless.Context.Utilities
{
    public class CollectionEntitySet<TEntity> : IEntitySet<TEntity>
        where TEntity : class
    {
        public CollectionEntitySet(ICollection<TEntity> collection)
        {
            var queryable = collection.AsQueryable();

            Queryable = new AsyncQueryable<TEntity>(
                queryable.Provider.AsAsyncQueryProvider(),
                Expression.Constant(queryable));
        }

        protected AsyncQueryable<TEntity> Queryable { get; private set; }

        public Type ElementType => typeof(TEntity);

        public Expression Expression => Queryable.Expression;

        public IQueryProvider Provider => Queryable.Provider;

        public IEnumerator<TEntity> GetEnumerator() => Queryable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Queryable.GetEnumerator();

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken token = default)
            => Queryable.GetAsyncEnumerator(token);
    }
}
