using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnstableSort.Crudless.Configuration.Builders;
using UnstableSort.Crudless.Configuration.Builders.Filter;

// ReSharper disable once CheckNamespace
namespace UnstableSort.Crudless.Configuration
{
    public static class ExtendedFilterExtensions
    {
        /// <summary>
        /// Adds a request filter that includes entities where the request's requestValue is
        /// equal to the entity's entityValue:
        ///     ie: Where(entity => request.requestValue == entity.entityValue)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
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

        /// <summary>
        /// Adds a request filter that includes entities where the provided constant value is
        /// equal to the entity's entityValue:
        ///     ie: Where(entity => value == entity.entityValue)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
        public static TBuilder AddEqualFilter<TBuilder, TRequest, TEntity, TValue, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            TValue value,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildUnaryFilter<TRequest, TEntity, TEntityProp>(entityValue,
                expr => Expression.Equal(Expression.Constant(value, typeof(TValue)), expr));

            return config.AddFilter(filter, condition);
        }

        /// <summary>
        /// Adds a request filter that includes entities where the request's requestValue is
        /// not equal to the entity's entityValue:
        ///     ie: Where(entity => request.requestValue != entity.entityValue)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
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

        /// <summary>
        /// Adds a request filter that includes entities where the provided constant value is
        /// not equal to the entity's entityValue:
        ///     ie: Where(entity => value != entity.entityValue)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
        public static TBuilder AddNotEqualFilter<TBuilder, TRequest, TEntity, TValue, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            TValue value,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildUnaryFilter<TRequest, TEntity, TEntityProp>(entityValue,
                expr => Expression.NotEqual(Expression.Constant(value, typeof(TValue)), expr));

            return config.AddFilter(filter, condition);
        }

        /// <summary>
        /// Adds a request filter that includes entities where the request's requestValue is
        /// less-than the entity's entityValue:
        ///     ie: Where(entity => request.requestValue < entity.entityValue)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
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

        /// <summary>
        /// Adds a request filter that includes entities where the provided constant value is
        /// less-than the entity's entityValue:
        ///     ie: Where(entity => value < entity.entityValue)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
        public static TBuilder AddLessThanFilter<TBuilder, TRequest, TEntity, TValue, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            TValue value,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildUnaryFilter<TRequest, TEntity, TEntityProp>(entityValue,
                expr => Expression.LessThan(Expression.Constant(value, typeof(TValue)), expr));

            return config.AddFilter(filter, condition);
        }

        /// <summary>
        /// Adds a request filter that includes entities where the request's requestValue is
        /// less-than-or-equal to the entity's entityValue:
        ///     ie: Where(entity => request.requestValue <= entity.entityValue)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
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

        /// <summary>
        /// Adds a request filter that includes entities where the provided constant value is
        /// less-than-or-equal to the entity's entityValue:
        ///     ie: Where(entity => value <= entity.entityValue)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
        public static TBuilder AddLessThanOrEqualFilter<TBuilder, TRequest, TEntity, TValue, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            TValue value,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildUnaryFilter<TRequest, TEntity, TEntityProp>(entityValue,
                expr => Expression.LessThanOrEqual(Expression.Constant(value, typeof(TValue)), expr));

            return config.AddFilter(filter, condition);
        }

        /// <summary>
        /// Adds a request filter that includes entities where the request's requestValue is
        /// greater-than the entity's entityValue:
        ///     ie: Where(entity => request.requestValue > entity.entityValue)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
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

        /// <summary>
        /// Adds a request filter that includes entities where the provided constant value is
        /// greater-than the entity's entityValue:
        ///     ie: Where(entity => value > entity.entityValue)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
        public static TBuilder AddGreaterThanFilter<TBuilder, TRequest, TEntity, TValue, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            TValue value,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildUnaryFilter<TRequest, TEntity, TEntityProp>(entityValue,
                expr => Expression.GreaterThan(Expression.Constant(value, typeof(TValue)), expr));

            return config.AddFilter(filter, condition);
        }

        /// <summary>
        /// Adds a request filter that includes entities where the request's requestValue is
        /// greater-than-or-equal to the entity's entityValue:
        ///     ie: Where(entity => request.requestValue >= entity.entityValue)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
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

        /// <summary>
        /// Adds a request filter that includes entities where the provided constant value is
        /// greater-than-or-equal to the entity's entityValue:
        ///     ie: Where(entity => value >= entity.entityValue)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
        public static TBuilder AddGreaterThanOrEqualFilter<TBuilder, TRequest, TEntity, TValue, TEntityProp>(
            this RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> config,
            TValue value,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<TRequest, bool> condition = null)
            where TEntity : class
            where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var filter = FilterHelpers.BuildUnaryFilter<TRequest, TEntity, TEntityProp>(entityValue,
                expr => Expression.GreaterThanOrEqual(Expression.Constant(value, typeof(TValue)), expr));

            return config.AddFilter(filter, condition);
        }

        /// <summary>
        /// Adds a request filter that includes entities where the entity's entityValue is true:
        ///     ie: Where(entity => entity.entityValue == true)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
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

        /// <summary>
        /// Adds a request filter that includes entities where the entity's entityValue is false:
        ///     ie: Where(entity => entity.entityValue == false)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
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

        /// <summary>
        /// Adds a request filter that includes entities where the entity's entityValue is NULL:
        ///     ie: Where(entity => entity.entityValue == null)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
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

        /// <summary>
        /// Adds a request filter that includes entities where the entity's entityValue is not NULL:
        ///     ie: Where(entity => entity.entityValue != null)
        /// </summary>
        /// <param name="condition">If condition is supplied, then the filter will only be applied when condition evaluates to true</param>
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