using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UnstableSort.Crudless.Context;
using UnstableSort.Crudless.Integration.EntityFrameworkCore;
using UnstableSort.Crudless.Integration.EntityFrameworkExtensions.Configuration;
using UnstableSort.Crudless.Integration.EntityFrameworkExtensions.Extensions;
using UnstableSort.Crudless.Tests.Fakes;

namespace UnstableSort.Crudless.Tests.Utilities
{
    public class SoftDeleteDataAgent : IDeleteDataAgent, IBulkDeleteDataAgent
    {
        public Task<TEntity> DeleteAsync<TEntity>(DataContext<TEntity> context,
            TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            var set = context.EntitySet.Implementation as EntityFrameworkEntitySet<TEntity>;
            var entry = set.Context.Entry(entity);

            if (entity is IEntity ientity)
            {
                ientity.IsDeleted = true;
                entry.State = EntityState.Modified;
            }
            else
            {
                entry.State = EntityState.Deleted;
            }

            token.ThrowIfCancellationRequested();

            return Task.FromResult(entry.Entity);
        }

        public async Task<TEntity[]> DeleteAsync<TEntity>(DataContext<TEntity> context,
            IEnumerable<TEntity> items,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            token.ThrowIfCancellationRequested();

            var set = context.EntitySet.Implementation as EntityFrameworkEntitySet<TEntity>;
            var entities = items.ToArray();

            if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
            {
                foreach (var entity in entities)
                {
                    ((IEntity)entity).IsDeleted = true;
                    set.Context.Entry(entity).State = EntityState.Modified;
                }

                await set.Context.SaveChangesAsync(token);
            }
            else
            {
                set.Context.Set<TEntity>().RemoveRange(entities);
            }

            return entities;
        }
    }
}
