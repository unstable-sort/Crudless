using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnstableSort.Crudless.Configuration.Builders;

// ReSharper disable once CheckNamespace
namespace UnstableSort.Crudless.Configuration
{
    public static class SelectorExtensions
    {
        public static TBuilder SelectUsing<TRequest, TEntity, TBuilder>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Func<TRequest, Expression<Func<TEntity, bool>>> selector)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return config.SelectWith(builder => builder.Single(selector));
        }

        public static TBuilder SelectUsing<TRequest, TEntity, TBuilder>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, TEntity, bool>> selector)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return config.SelectWith(builder => builder.Single(selector));
        }

        public static TBuilder SelectUsing<TRequest, TEntity, TBuilder, TKey>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, ICollection<TKey>>> requestEnumerableExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return config.SelectWith(builder => builder.Collection(requestEnumerableExpr, entityKeyExpr));
        }

        public static TBuilder SelectUsing<TRequest, TEntity, TBuilder, TIn, TKey>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, ICollection<TIn>>> requestEnumerableExpr,
            Expression<Func<TIn, TKey>> requestItemKeyExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return config.SelectWith(builder => builder.Collection(requestEnumerableExpr, requestItemKeyExpr, entityKeyExpr));
        }
    }
}
