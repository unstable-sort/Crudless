using System;
using System.Linq;
using System.Linq.Expressions;
using UnstableSort.Crudless.Configuration.Builders;
using UnstableSort.Crudless.Configuration.Builders.Select;
using UnstableSort.Crudless.Exceptions;

// ReSharper disable once CheckNamespace
namespace UnstableSort.Crudless.Configuration
{
    public static class StandardSelectorExtensions
    {
        /// <summary>
        /// Selects the entity that satisfies the selector-returned predicate:
        ///     ie: Single(request => entity => selector'(entity) == true)
        /// </summary>
        public static RequestEntityConfigBuilder<TRequest, TEntity> SelectBy<TRequest, TEntity>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            Func<TRequest, Expression<Func<TEntity, bool>>> selector)
            where TEntity : class
                => config.SetSelector(Selector.From(selector));

        /// <summary>
        /// Selects the entity that satisfies the predicate:
        ///     ie: Single((request, entity) => selector(request, entity) == true)
        /// </summary>
        public static RequestEntityConfigBuilder<TRequest, TEntity> SelectBy<TRequest, TEntity>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, TEntity, bool>> selector)
            where TEntity : class
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");
            var body = selector.Body.ReplaceParameter(selector.Parameters[0], rParamExpr);

            var selectorClause = Expression.Quote(Expression.Lambda<Func<TEntity, bool>>(body, selector.Parameters[1]));
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return config.SetSelector(Selector.From(selectorLambda.Compile()));
        }

        /// <summary>
        /// Selects the entity whose entityKey member values are equal to the request's requestKey member values:
        ///     ie: Single(entity => request.requestKeys[0] == entity.entityKeys[0] && request.requestKeys[1] == entity.entityKeys[1] && ...)
        /// </summary>
        public static RequestEntityConfigBuilder<TRequest, TEntity> SelectBy<TRequest, TEntity, TRequestKey, TEntityKey>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, TRequestKey>> requestKeys,
            Expression<Func<TEntity, TEntityKey>> entityKeys)
            where TEntity : class
        {
            var kRequest = Key.MakeKeys(requestKeys);
            var kEntity = Key.MakeKeys(entityKeys);

            if (kRequest == null || kRequest.Length == 0 ||
                kEntity == null || kEntity.Length == 0)
                return config;

            if (kRequest.Length != kEntity.Length)
                throw new IncompatibleKeysException(typeof(TRequest), typeof(TEntity));

            var selector = SelectorHelpers.BuildSingle<TRequest, TEntity>(kRequest.Zip(kEntity, (r, e) => ((IKey)r, (IKey)e)));

            return config.SetSelector(selector);
        }

        /// <summary>
        /// Selects the entity whose entityKey member value is equal to the request's requestKey member value:
        ///     ie: Single(entity => request.requestKey == entity.entityKey)
        /// </summary>
        public static RequestEntityConfigBuilder<TRequest, TEntity> SelectBy<TRequest, TEntity, TRequestKey>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            Expression<Func<TRequest, TRequestKey>> requestKey,
            string entityKey)
            where TEntity : class
        {
            var kRequest = Key.MakeKeys(requestKey);
            var kEntity = Key.MakeKey<TEntity>(entityKey);

            if (kRequest == null || kRequest.Length == 0 || kEntity == null)
                return config;

            if (kRequest.Length != 1)
                throw new IncompatibleKeysException(typeof(TRequest), typeof(TEntity));

            var selector = SelectorHelpers.BuildSingle<TRequest, TEntity>(kRequest[0], kEntity);

            return config.SetSelector(selector);
        }

        /// <summary>
        /// Selects the entity whose entityKey member value is equal to the request's requestKey member value:
        ///     ie: Single(entity => request.requestKey == entity.entityKey)
        /// </summary>
        public static RequestEntityConfigBuilder<TRequest, TEntity> SelectBy<TRequest, TEntity, TEntityKey>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            string requestKey,
            Expression<Func<TRequest, TEntityKey>> entityKey)
            where TEntity : class
        {
            var kRequest = Key.MakeKey<TRequest>(requestKey);
            var kEntity = Key.MakeKeys(entityKey);

            if (kEntity == null || kEntity.Length == 0 || kRequest == null)
                return config;

            if (kEntity.Length != 1)
                throw new IncompatibleKeysException(typeof(TRequest), typeof(TEntity));

            var selector = SelectorHelpers.BuildSingle<TRequest, TEntity>(kRequest, kEntity[0]);

            return config.SetSelector(selector);
        }

        /// <summary>
        /// Selects the entity whose entityKey member value is equal to the request's requestKey member value:
        ///     ie: Single(entity => request.requestKey == entity.entityKey)
        /// </summary>
        public static RequestEntityConfigBuilder<TRequest, TEntity> SelectBy<TRequest, TEntity>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            string requestKey, 
            string entityKey)
            where TEntity : class
        {
            var kRequest = Key.MakeKey<TRequest>(requestKey);
            var kEntity = Key.MakeKey<TEntity>(entityKey);

            var selector = SelectorHelpers.BuildSingle<TRequest, TEntity>(kRequest, kEntity);

            return config.SetSelector(selector);
        }

        /// <summary>
        /// Selects the entity whose entityKey member values are equal to the request's requestKey member values:
        ///     ie: Single(entity => request.requestKeys[0] == entity.entityKeys[0] && request.requestKeys[1] == entity.entityKeys[1] && ...)
        /// </summary>
        public static RequestEntityConfigBuilder<TRequest, TEntity> SelectBy<TRequest, TEntity>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            string[] requestKeys, 
            string[] entityKeys)
            where TEntity : class
        {
            var kRequest = requestKeys.Select(Key.MakeKey<TRequest>).ToArray();
            var kEntity = entityKeys.Select(Key.MakeKey<TEntity>).ToArray();

            if (kRequest == null || kRequest.Length == 0 ||
                kEntity == null || kEntity.Length == 0)
                return config;

            if (kRequest.Length != kEntity.Length)
                throw new IncompatibleKeysException(typeof(TRequest), typeof(TEntity));

            var selector = SelectorHelpers.BuildSingle<TRequest, TEntity>(kRequest.Zip(kEntity, (r, e) => ((IKey)r, (IKey)e)));

            return config.SetSelector(selector);
        }

        /// <summary>
        /// Selects the entity whose key member value is equal to the request's key member value:
        ///     ie: Single(entity => request.key == entity.key)
        /// </summary>
        public static RequestEntityConfigBuilder<TRequest, TEntity> SelectBy<TRequest, TEntity>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            string key)
            where TEntity : class
        {
            var kRequest = Key.MakeKey<TRequest>(key);
            var kEntity = Key.MakeKey<TEntity>(key);

            var selector = SelectorHelpers.BuildSingle<TRequest, TEntity>(kRequest, kEntity);

            return config.SetSelector(selector);
        }

        /// <summary>
        /// Selects the entity whose key member values are equal to the request's key member values:
        ///     ie: Single(entity => request.keys[0] == entity.keys[0] && request.keys[1] == entity.keys[1] && ...)
        /// </summary>
        public static RequestEntityConfigBuilder<TRequest, TEntity> SelectBy<TRequest, TEntity>(
            this RequestEntityConfigBuilder<TRequest, TEntity> config,
            string[] keys)
            where TEntity : class
        {
            if (keys == null || keys.Length == 0 || keys.Any(string.IsNullOrWhiteSpace))
                return config;

            var kRequest = keys.Select(Key.MakeKey<TRequest>).ToArray();
            var kEntity = keys.Select(Key.MakeKey<TEntity>).ToArray();

            var selector = SelectorHelpers.BuildSingle<TRequest, TEntity>(kRequest.Zip(kEntity, (r, e) => ((IKey)r, (IKey)e)));

            return config.SetSelector(selector);
        }
    }
}
