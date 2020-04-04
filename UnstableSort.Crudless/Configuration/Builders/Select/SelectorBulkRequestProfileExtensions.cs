using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnstableSort.Crudless.Configuration.Builders;
using UnstableSort.Crudless.Configuration.Builders.Select;
using UnstableSort.Crudless.Exceptions;

// ReSharper disable once CheckNamespace
namespace UnstableSort.Crudless.Configuration
{
    public static class SelectorBulkRequestProfileExtensions
    {
        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> SelectBy<TRequest, TItem, TEntity>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Expression<Func<TRequest, ICollection<TItem>>> requestItems,
            Expression<Func<TEntity, TItem>> entityKey)
            where TEntity : class
        {
            var kEntity = Key.MakeKeys(entityKey);
            var kItem = Key.Identity<TItem>();

            if (kEntity.Length > 1)
                throw new BadConfigurationException($"Composite keys are not supported for bulk requests");

            var selector = SelectorHelpers.BuildCollection<TRequest, TItem, TEntity>(requestItems, kEntity[0], kItem);

            return config.SetSelector(selector);
        }

        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> SelectBy<TRequest, TItem, TEntity>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Expression<Func<TRequest, ICollection<TItem>>> requestItems,
            string entityKey)
            where TEntity : class
        {
            var kEntity = Key.MakeKey<TEntity>(entityKey);
            var kItem = Key.Identity<TItem>();

            var selector = SelectorHelpers.BuildCollection<TRequest, TItem, TEntity>(requestItems, kEntity, kItem);

            return config.SetSelector(selector);
        }

        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> SelectBy<TRequest, TItem, TEntity, TKey>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Expression<Func<TRequest, ICollection<TItem>>> requestItems,
            Expression<Func<TItem, TKey>> itemKey,
            Expression<Func<TEntity, TKey>> entityKey)
            where TEntity : class
        {
            var kEntity = Key.MakeKeys(entityKey);
            var kItem = Key.MakeKeys(itemKey);

            if (kItem.Length != kEntity.Length)
                throw new IncompatibleKeysException(typeof(TRequest), typeof(TEntity));

            if (kEntity.Length > 1 || kItem.Length > 1)
                throw new BadConfigurationException($"Composite keys are not supported for bulk requests");

            var selector = SelectorHelpers.BuildCollection<TRequest, TItem, TEntity>(requestItems, kEntity[0], kItem[0]);

            return config.SetSelector(selector);
        }

        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> SelectBy<TRequest, TItem, TEntity>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Expression<Func<TRequest, ICollection<TItem>>> requestItems,
            string itemKey,
            string entityKey)
            where TEntity : class
        {
            var kEntity = Key.MakeKey<TEntity>(entityKey);
            var kItem = Key.MakeKey<TItem>(itemKey);

            var selector = SelectorHelpers.BuildCollection<TRequest, TItem, TEntity>(requestItems, kEntity, kItem);

            return config.SetSelector(selector);
        }
    }
}
