using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace UnstableSort.Crudless.Context.Utilities
{
    public class AsyncQueryable<TEntity> : IQueryable, IQueryable<TEntity>, IOrderedQueryable<TEntity>, IAsyncEnumerable<TEntity>
    {
        protected readonly IAsyncQueryProvider QueryProvider;

        public AsyncQueryable(IQueryProvider queryProvider, Expression expression)
        {
            QueryProvider = queryProvider.AsAsyncQueryProvider()
                ?? throw new ArgumentNullException(nameof(queryProvider));

            Expression = expression
                ?? throw new ArgumentNullException(nameof(expression));
        }

        public virtual Type ElementType => typeof(TEntity);

        public virtual Expression Expression { get; }

        public virtual IQueryProvider Provider => QueryProvider;

        public virtual IEnumerator<TEntity> GetEnumerator()
            => QueryProvider.Execute<IEnumerable<TEntity>>(Expression).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => QueryProvider.Execute<IEnumerable>(Expression).GetEnumerator();

        public virtual IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken token = default)
            => QueryProvider.ExecuteAsync<IAsyncEnumerable<TEntity>>(Expression).GetAsyncEnumerator(token);

        #if DEBUG
            public virtual string DebugView => Expression.ToString();
        #endif
    }
}
