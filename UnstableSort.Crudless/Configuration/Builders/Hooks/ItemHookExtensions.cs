using System;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration.Builders;

// ReSharper disable once CheckNamespace
namespace UnstableSort.Crudless.Configuration
{
    public static class ItemHookExtensions
    {
        /// <summary>
        /// Adds an item hook from the provided method.
        /// </summary>
        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> AddItemHook<TRequest, TItem, TEntity>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Func<TRequest, TItem, CancellationToken, Task<TItem>> hook)
            where TEntity : class
        {
            return config.AddItemHook(FunctionItemHookFactory.From(hook));
        }

        /// <summary>
        /// Adds an item hook from the provided method.
        /// </summary>
        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> AddItemHook<TRequest, TItem, TEntity>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Func<TRequest, TItem, Task<TItem>> hook)
            where TEntity : class
        {
            return config.AddItemHook(FunctionItemHookFactory.From(hook));
        }

        /// <summary>
        /// Adds an item hook from the provided method.
        /// </summary>
        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> AddItemHook<TRequest, TItem, TEntity>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Func<TRequest, TItem, TItem> hook)
            where TEntity : class
        {
            return config.AddItemHook(FunctionItemHookFactory.From(hook));
        }

        /// <summary>
        /// Adds an item hook from the provided method.
        /// </summary>
        public static BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> AddItemHook<TRequest, TItem, TEntity>(
            this BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> config,
            Func<TItem, TItem> hook)
            where TEntity : class
        {
            return config.AddItemHook((_, item) => hook(item));
        }
    }
}
