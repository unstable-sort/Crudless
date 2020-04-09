using System;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration.Builders;

// ReSharper disable once CheckNamespace
namespace UnstableSort.Crudless.Configuration
{
    public static class AuditHookExtensions
    {
        /// <summary>
        /// Adds an audit hook from the provided method.
        /// </summary>
        public static TBuilder AddAuditHook<TBuilder, TRequest, TEntity>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Func<TRequest, TEntity, TEntity, CancellationToken, Task> hook)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return config.AddAuditHook(FunctionAuditHookFactory.From(hook));
        }

        /// <summary>
        /// Adds an audit hook from the provided method.
        /// </summary>
        public static TBuilder AddAuditHook<TBuilder, TRequest, TEntity>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Func<TRequest, TEntity, TEntity, Task> hook)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return config.AddAuditHook(FunctionAuditHookFactory.From(hook));
        }

        /// <summary>
        /// Adds an audit hook from the provided method.
        /// </summary>
        public static TBuilder AddAuditHook<TBuilder, TRequest, TEntity>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Action<TRequest, TEntity, TEntity> hook)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return config.AddAuditHook(FunctionAuditHookFactory.From(hook));
        }
    }
}
