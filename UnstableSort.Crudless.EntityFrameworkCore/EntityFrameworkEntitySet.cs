using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using UnstableSort.Crudless.Context;

namespace UnstableSort.Crudless.EntityFrameworkCore
{
    public class EntityFrameworkEntitySet<TEntity> : IEntitySet<TEntity>
        where TEntity : class
    {
        public EntityFrameworkEntitySet(DbContext context)
        {
            Context = context;
            Set = Context.Set<TEntity>();
            Queryable = Set.AsQueryable();
        }

        public DbContext Context { get; }

        public DbSet<TEntity> Set { get; }

        public IQueryable<TEntity> Queryable { get; }

        public Type ElementType => Queryable.ElementType;

        public Expression Expression => Queryable.Expression;

        public IQueryProvider Provider => Queryable.Provider;

        public IEnumerator<TEntity> GetEnumerator() => Queryable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Queryable.GetEnumerator();

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken token = default)
            => Set.AsAsyncEnumerable().GetAsyncEnumerator(token);
    }
}
