using System;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration.Builders;

// ReSharper disable once CheckNamespace
namespace UnstableSort.Crudless.Configuration
{
    public static class EntityHookExtensions
    {
        /// <summary>
        /// Adds an entity hook from the provided method.
        /// </summary>
        public static TBuilder AddEntityHook<TBuilder, TRequest, TEntity>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Func<TRequest, TEntity, CancellationToken, Task> hook)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return config.AddEntityHook(FunctionEntityHookFactory.From(hook));
        }

        /// <summary>
        /// Adds an entity hook from the provided method.
        /// </summary>
        public static TBuilder AddEntityHook<TBuilder, TRequest, TEntity>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Func<TRequest, TEntity, Task> hook)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return config.AddEntityHook(FunctionEntityHookFactory.From(hook));
        }

        /// <summary>
        /// Adds an entity hook from the provided method.
        /// </summary>
        public static TBuilder AddEntityHook<TBuilder, TRequest, TEntity>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Action<TRequest, TEntity> hook)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return config.AddEntityHook(FunctionEntityHookFactory.From(hook));
        }
    }
}
