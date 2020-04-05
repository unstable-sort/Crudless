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
                var entries = set.Context.ChangeTracker
                    .Entries()
                    .Where(x => entities.Contains(x.Entity) && x.State == EntityState.Deleted)
                    .ToArray();

                foreach (var entry in entries)
                    entry.State = EntityState.Detached;

                if (set.Context.ChangeTracker.Entries().Any(x => x.Entity is TEntity))
                    await set.Context.SaveChangesAsync(token);

                foreach (var entity in entities.Cast<IEntity>())
                    entity.IsDeleted = true;

                await set.Context.BulkUpdateAsync(entities,
                    operation => operation.Configure(BulkConfigurationType.Delete, context),
                    token);
            }
            else
            {
                await set.Context.BulkDeleteAsync(entities,
                    operation => operation.Configure(BulkConfigurationType.Delete, context),
                    token);
            }

            return entities;
        }
    }
}
