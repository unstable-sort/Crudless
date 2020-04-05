using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnstableSort.Crudless.Configuration.Builders;
using UnstableSort.Crudless.Configuration.Builders.Filter;

// ReSharper disable once CheckNamespace
namespace UnstableSort.Crudless.Configuration
{
    public static class FilterCommonRequestProfileExtensions
    {
        public static TBuilder AddFilter<TBuilder, TRequest, TEntity>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TEntity, bool>> filter,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return condition == null
                ? config.AddRequestFilter((_, queryable) => queryable.Where(filter))
                : config.AddRequestFilter((request, queryable) =>
                    condition(request) ? queryable.Where(filter) : queryable);
        }

        public static TBuilder AddFilter<TBuilder, TRequest, TEntity>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Func<TRequest, Expression<Func<TEntity, bool>>> filter,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            return condition == null
                ? config.AddRequestFilter((request, queryable) =>
                    queryable.Where(filter(request)))
                : config.AddRequestFilter((request, queryable) =>
                    condition(request) ? queryable.Where(filter(request)) : queryable);
        }

        public static TBuilder AddFilter<TBuilder, TRequest, TEntity>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, TEntity, bool>> filter,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");
            var body = filter.Body.ReplaceParameter(filter.Parameters[0], rParamExpr);
            var filterClause = Expression.Lambda<Func<TEntity, bool>>(body, filter.Parameters[1]);
            var filterLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(filterClause, rParamExpr);

            return config.AddFilter(filterLambda.Compile(), condition);
        }

        public static TBuilder AddFilter<TBuilder, TRequest, TEntity, TRequestProp, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, TRequestProp>> requestValue,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Expression<Func<TRequestProp, TEntityProp, bool>> comparator,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");
            var eParamExpr = Expression.Parameter(typeof(TEntity), "e");
            
            var rValueExpr = Expression.Invoke(requestValue, rParamExpr);
            var eValueExpr = Expression.Invoke(entityValue, eParamExpr);

            var compareExpr = Expression.Invoke(comparator, rValueExpr, eValueExpr);

            var filterClause = Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            var filterLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(filterClause, rParamExpr);

            return config.AddFilter(filterLambda.Compile(), condition);
        }

        public static TBuilder AddContainsFilter<TBuilder, TRequest, TEntity, TItem>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, ICollection<TItem>>> requestValue,
            Expression<Func<TEntity, TItem>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");
            var eParamExpr = Expression.Parameter(typeof(TEntity), "e");

            var rCollectionExpr = Expression.Invoke(requestValue, rParamExpr);
            var eValueExpr = Expression.Invoke(entityValue, eParamExpr);

            var containsInfo = typeof(Enumerable)
                .GetMethods()
                .Single(x => x.Name == nameof(Enumerable.Contains) && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TItem));

            var rContainsExpr = Expression.Call(containsInfo, rCollectionExpr, eValueExpr);

            var filterClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);
            var filterLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(filterClause, rParamExpr);

            return config.AddFilter(filterLambda.Compile(), condition);
        }

        public static TBuilder AddContainsFilter<TBuilder, TRequest, TEntity, TItem>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, ICollection<TItem>>> requestValue,
            string entityValueMember,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");
            var eParamExpr = Expression.Parameter(typeof(TEntity), "e");

            var rCollectionExpr = Expression.Invoke(requestValue, rParamExpr);
            var eValueExpr = Expression.PropertyOrField(eParamExpr, entityValueMember);

            var containsInfo = typeof(Enumerable)
                .GetMethods()
                .Single(x => x.Name == nameof(Enumerable.Contains) && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TItem));

            var rContainsExpr = Expression.Call(containsInfo, rCollectionExpr, eValueExpr);

            var filterClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);
            var filterLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(filterClause, rParamExpr);

            return config.AddFilter(filterLambda.Compile(), condition);
        }

        public static TBuilder AddEqualFilter<TBuilder, TRequest, TEntity, TRequestProp, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, TRequestProp>> requestValue,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildBinaryFilter(requestValue, entityValue, Expression.Equal);

            return config.AddFilter(filter, condition);
        }

        public static TBuilder AddNotEqualFilter<TBuilder, TRequest, TEntity, TRequestProp, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, TRequestProp>> requestValue,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildBinaryFilter(requestValue, entityValue, Expression.NotEqual);

            return config.AddFilter(filter, condition);
        }

        public static TBuilder AddLessThanFilter<TBuilder, TRequest, TEntity, TRequestProp, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, TRequestProp>> requestValue,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildBinaryFilter(requestValue, entityValue, Expression.LessThan);

            return config.AddFilter(filter, condition);
        }

        public static TBuilder AddLessThanOrEqualFilter<TBuilder, TRequest, TEntity, TRequestProp, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, TRequestProp>> requestValue,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildBinaryFilter(requestValue, entityValue, Expression.LessThanOrEqual);

            return config.AddFilter(filter, condition);
        }

        public static TBuilder AddGreaterThanFilter<TBuilder, TRequest, TEntity, TRequestProp, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, TRequestProp>> requestValue,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildBinaryFilter(requestValue, entityValue, Expression.GreaterThan);

            return config.AddFilter(filter, condition);
        }

        public static TBuilder AddGreaterThanOrEqualFilter<TBuilder, TRequest, TEntity, TRequestProp, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TRequest, TRequestProp>> requestValue,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildBinaryFilter(requestValue, entityValue, Expression.GreaterThanOrEqual);

            return config.AddFilter(filter, condition);
        }

        public static TBuilder AddTrueFilter<TBuilder, TRequest, TEntity, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildUnaryFilter<TRequest, TEntity, TEntityProp>(entityValue, 
                expr => Expression.Equal(expr, Expression.Constant(true, typeof(bool))));

            return config.AddFilter(filter, condition);
        }

        public static TBuilder AddFalseFilter<TBuilder, TRequest, TEntity, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildUnaryFilter<TRequest, TEntity, TEntityProp>(entityValue,
                expr => Expression.Equal(expr, Expression.Constant(false, typeof(bool))));

            return config.AddFilter(filter, condition);
        }

        public static TBuilder AddNullFilter<TBuilder, TRequest, TEntity, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildUnaryFilter<TRequest, TEntity, TEntityProp>(entityValue, 
                expr => Expression.Equal(expr, Expression.Constant(default(TEntityProp), typeof(TEntityProp))));

            return config.AddFilter(filter, condition);
        }

        public static TBuilder AddNotNullFilter<TBuilder, TRequest, TEntity, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildUnaryFilter<TRequest, TEntity, TEntityProp>(entityValue,
                expr => Expression.NotEqual(expr, Expression.Constant(default(TEntityProp), typeof(TEntityProp))));

            return config.AddFilter(filter, condition);
        }
    }
}