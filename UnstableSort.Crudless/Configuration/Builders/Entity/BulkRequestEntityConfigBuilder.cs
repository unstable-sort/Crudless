using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration.Builders.Select;
using UnstableSort.Crudless.Exceptions;
using UnstableSort.Crudless.Requests;

// ReSharper disable once CheckNamespace
namespace UnstableSort.Crudless.Configuration.Builders
{
    public class BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>
        : RequestEntityConfigBuilderCommon<TRequest, TEntity, BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>>
        where TEntity : class
    {
        private Expression<Func<TRequest, ICollection<TItem>>> _getRequestItems;

        private readonly List<IItemHookFactory> _itemHooks
            = new List<IItemHookFactory>();

        /// <summary>
        /// Applies this profile to the provided configuration.
        /// This method is not intended to be used externally.
        /// </summary>
        public override void Build<TCompatibleRequest>(RequestConfig<TCompatibleRequest> config)
        {
            if (_getRequestItems == null)
            {
                var message =
                    $"No request item source has been defined for '{typeof(TRequest)}'." +
                    $"Define item source by calling `{nameof(HasRequestItems)}` in the request's profile.";

                throw new BadConfigurationException(message);
            }

            base.Build(config);

            if (Selector == null)
                BuildDefaultSelector(config);

            BuildJoiner(config);

            config.AddItemHooksFor<TEntity>(_itemHooks);
        }

        /// <summary>
        /// Provides request handlers with how to retrieve the items on which it will operate.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> HasRequestItems(
            Expression<Func<TRequest, ICollection<TItem>>> requestItemsCollection)
        {
            _getRequestItems = requestItemsCollection;

            RequestItemSource = UnstableSort.Crudless.RequestItemSource.From(BuildItemSource(requestItemsCollection));

            return this;
        }

        /// <summary>
        /// Provides a request item's "key" members.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UseRequestItemKey<TKey>(
            Expression<Func<TItem, TKey>> itemKeyExpr)
        {
            RequestItemKeys = new[] { new Key(typeof(TKey), itemKeyExpr) };

            return this;
        }

        /// <summary>
        /// Provides a request item's "key" member.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UseRequestItemKey(
            string itemKeyProperty)
        {
            RequestItemKeys = new[] { Key.MakeKey<TItem>(itemKeyProperty) };

            return this;
        }

        /// <summary>
        /// Provides a request item's "key" members.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UseRequestItemKey(
            string[] itemKeyMembers)
        {
            RequestItemKeys = itemKeyMembers.Select(Key.MakeKey<TItem>).ToArray();

            return this;
        }

        /// <summary>
        /// Provides request handlers with how to create a new entity from a request item.
        /// The default method is to resolve an IMapper and map the item into a new TEntity.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> CreateEntityWith(
            Func<RequestContext<TRequest>, TItem, CancellationToken, Task<TEntity>> creator)
        {
            CreateEntity = (context, item, ct) => creator(context.Cast<TRequest>(), (TItem)item, ct);

            return this;
        }

        /// <summary>
        /// Provides request handlers with how to create a new entity from a request item.
        /// The default method is to resolve an IMapper and map the item into a new TEntity.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> CreateEntityWith(
            Func<RequestContext<TRequest>, TItem, Task<TEntity>> creator)
            => CreateEntityWith((context, item, ct) => creator(context, item));

        /// <summary>
        /// Provides request handlers with how to create a new entity from a request item.
        /// The default method is to resolve an IMapper and map the item into a new TEntity.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> CreateEntityWith(
            Func<RequestContext<TRequest>, TItem, TEntity> creator)
        {
            CreateEntity = (context, item, ct) =>
            {
                if (ct.IsCancellationRequested)
                    return Task.FromCanceled<TEntity>(ct);

                return Task.FromResult(creator(context.Cast<TRequest>(), (TItem)item));
            };

            return this;
        }

        /// <summary>
        /// Provides request handlers with how to create a new entity from a request item.
        /// The default method is to resolve an IMapper and map the item into a new TEntity.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> CreateEntityWith(
            Func<TItem, CancellationToken, Task<TEntity>> creator)
        {
            CreateEntity = (context, item, ct) => creator((TItem)item, ct);

            return this;
        }

        /// <summary>
        /// Provides request handlers with how to create a new entity from a request item.
        /// The default method is to resolve an IMapper and map the item into a new TEntity.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> CreateEntityWith(
            Func<TItem, Task<TEntity>> creator)
            => CreateEntityWith((item, ct) => creator(item));

        /// <summary>
        /// Provides request handlers with how to create a new entity from a request item.
        /// The default method is to resolve an IMapper and map the item into a new TEntity.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> CreateEntityWith(
            Func<TItem, TEntity> creator)
        {
            CreateEntity = (context, item, ct) =>
            {
                if (ct.IsCancellationRequested)
                    return Task.FromCanceled<TEntity>(ct);

                return Task.FromResult(creator((TItem)item));
            };

            return this;
        }

        /// <summary>
        /// Provides request handlers with how to update an entity from a request item.
        /// The default method is to resolve an IMapper and map the item on to the existing entity.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UpdateEntityWith(
            Func<RequestContext<TRequest>, TItem, TEntity, CancellationToken, Task<TEntity>> updator)
        {
            UpdateEntity = (context, item, entity, ct) => updator(context.Cast<TRequest>(), (TItem)item, entity, ct);

            return this;
        }

        /// <summary>
        /// Provides request handlers with how to update an entity from a request item.
        /// The default method is to resolve an IMapper and map the item on to the existing entity.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UpdateEntityWith(
            Func<RequestContext<TRequest>, TItem, TEntity, Task<TEntity>> updator)
            => UpdateEntityWith((context, item, entity, ct) => updator(context, item, entity));

        /// <summary>
        /// Provides request handlers with how to update an entity from a request item.
        /// The default method is to resolve an IMapper and map the item on to the existing entity.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UpdateEntityWith(
            Func<RequestContext<TRequest>, TItem, TEntity, TEntity> updator)
        {
            UpdateEntity = (context, item, entity, ct) =>
            {
                if (ct.IsCancellationRequested)
                    return Task.FromCanceled<TEntity>(ct);

                return Task.FromResult(updator(context.Cast<TRequest>(), (TItem)item, entity));
            };

            return this;
        }

        /// <summary>
        /// Provides request handlers with how to update an entity from a request item.
        /// The default method is to resolve an IMapper and map the item on to the existing entity.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UpdateEntityWith(
            Func<TItem, TEntity, CancellationToken, Task<TEntity>> updator)
        {
            UpdateEntity = (context, item, entity, ct) => updator((TItem)item, entity, ct);

            return this;
        }

        /// <summary>
        /// Provides request handlers with how to update an entity from a request item.
        /// The default method is to resolve an IMapper and map the item on to the existing entity.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UpdateEntityWith(
            Func<TItem, TEntity, Task<TEntity>> updator)
            => UpdateEntityWith((item, entity, ct) => updator(item, entity));

        /// <summary>
        /// Provides request handlers with how to update an entity from a request item.
        /// The default method is to resolve an IMapper and map the item on to the existing entity.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UpdateEntityWith(
            Func<TItem, TEntity, TEntity> updator)
        {
            UpdateEntity = (context, item, entity, ct) =>
            {
                if (ct.IsCancellationRequested)
                    return Task.FromCanceled<TEntity>(ct);

                return Task.FromResult(updator((TItem)item, entity));
            };

            return this;
        }

        /// <summary>
        /// Adds an item hook of the given type.
        /// The hook will be resolved through the service provider.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> AddItemHook(Type hookType)
        {
            var baseHookType = hookType
                .GetBaseTypes()
                .SingleOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ItemHook<,>));

            if (baseHookType == null)
                throw new ArgumentException($"Unable to add '{hookType}' as an audit hook for '{typeof(TRequest)}'.\r\n" +
                                            $"Item hooks must inherit ItemHook<TRequest, TItem>.");

            var requestType = baseHookType.GenericTypeArguments[0];
            if (!requestType.IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddItemHook), requestType, typeof(TRequest));

            var itemType = baseHookType.GenericTypeArguments[1];
            if (!itemType.IsAssignableFrom(typeof(TItem)))
                throw new ContravarianceException(nameof(AddItemHook), itemType, typeof(TItem));

            var factoryMethod = typeof(TypeItemHookFactory)
                .GetMethod(nameof(TypeItemHookFactory.From), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(hookType, requestType, itemType);

            try
            {
                return AddItemHook((IItemHookFactory)factoryMethod.Invoke(null, Array.Empty<object>()));
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw e;
            }
        }

        /// <summary>
        /// Adds an item hook instance.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> AddItemHook(IItemHook hook)
        {
            var hookType = hook.GetType();

            var baseHookType = hookType
                .GetBaseTypes()
                .SingleOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ItemHook<,>));

            if (baseHookType == null)
                throw new ArgumentException($"Unable to add '{hookType}' as an item hook for '{typeof(TRequest)}'.\r\n" +
                                            $"Item hooks must inherit ItemHook<TRequest, TItem>.");

            var requestType = baseHookType.GenericTypeArguments[0];
            if (!requestType.IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddItemHook), requestType, typeof(TRequest));

            var itemType = baseHookType.GenericTypeArguments[1];
            if (!itemType.IsAssignableFrom(typeof(TItem)))
                throw new ContravarianceException(nameof(AddItemHook), itemType, typeof(TItem));

            var factoryMethod = typeof(InstanceItemHookFactory)
                .GetMethod(nameof(InstanceItemHookFactory.From), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(requestType, itemType);

            try
            {
                return AddItemHook((IItemHookFactory)factoryMethod.Invoke(null, new object[] { hook }));
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw e;
            }
        }

        /// <summary>
        /// Adds an item hook of the given type.
        /// The hook will be resolved through the service provider.
        /// </summary>
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> AddItemHook<THook>()
            where THook : IItemHook
                => AddItemHook(typeof(THook));

        internal BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> AddItemHook(IItemHookFactory itemHookFactory)
        {
            _itemHooks.Add(itemHookFactory);

            return this;
        }

        private void BuildJoiner<TCompatibleRequest>(RequestConfig<TCompatibleRequest> config)
        {
            if (EntityKeys == null || EntityKeys.Length == 0 ||
                RequestItemKeys == null || RequestItemKeys.Length == 0)
                return;

            if (RequestItemKeys.Length != EntityKeys.Length)
                throw new IncompatibleKeysException(typeof(TCompatibleRequest), typeof(TEntity));

            if (RequestItemKeys.Length > 1)
                throw new BadConfigurationException($"Composite keys are not supported for bulk requests");

            var joinInfo = typeof(EnumerableExtensions)
                .GetMethod(nameof(EnumerableExtensions.FullOuterJoin), BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(typeof(object), typeof(TEntity), RequestItemKeys[0].KeyType);

            var makeKeySelectorInfo = typeof(BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>)
                .GetMethod(nameof(MakeKeySelector), BindingFlags.Static | BindingFlags.NonPublic);

            var itemsParam = Expression.Parameter(typeof(IEnumerable<object>));
            var entitiesParam = Expression.Parameter(typeof(IEnumerable<TEntity>));

            var makeLeftKeySelector = makeKeySelectorInfo.MakeGenericMethod(typeof(object), RequestItemKeys[0].KeyType);
            var convLeftKeyParam = Expression.Parameter(typeof(object));
            var convLeftKeyCall = Expression.Invoke(
                RequestItemKeys[0].KeyExpression,
                Expression.Convert(convLeftKeyParam, typeof(TItem)));
            var leftKeyExpr = Expression.Call(makeLeftKeySelector, Expression.Lambda(convLeftKeyCall, convLeftKeyParam));

            var makeRightKeySelector = makeKeySelectorInfo.MakeGenericMethod(typeof(TEntity), EntityKeys[0].KeyType);
            var rightKeyExpr = Expression.Call(makeRightKeySelector, Expression.Constant(EntityKeys[0].KeyExpression));

            var joinExpr = Expression.Call(joinInfo, itemsParam, entitiesParam, leftKeyExpr, rightKeyExpr);
            var lambdaExpr = Expression.Lambda<Func<IEnumerable<object>, IEnumerable<TEntity>, IEnumerable<Tuple<object, TEntity>>>>(
                joinExpr, itemsParam, entitiesParam);

            config.SetEntityJoiner(lambdaExpr.Compile());
        }

        private Func<TRequest, object> BuildItemSource(Expression<Func<TRequest, ICollection<TItem>>> itemsExpr)
        {
            var enumerableMethods = typeof(Enumerable).GetMethods();
            var rParamExpr = Expression.Parameter(typeof(TRequest));

            var castInfo = enumerableMethods
                .Single(x => x.Name == nameof(Enumerable.Cast) && x.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(object));
            var castExpr = Expression.Call(castInfo,
                Expression.Invoke(itemsExpr, rParamExpr));

            var toArrayInfo = enumerableMethods
                .Single(x => x.Name == nameof(Enumerable.ToArray) && x.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(object));
            var toArrayExpr = Expression.Call(toArrayInfo, castExpr);

            var lambdaExpr = Expression.Lambda<Func<TRequest, object>>(
                Expression.Convert(toArrayExpr, typeof(object)), rParamExpr);

            return lambdaExpr.Compile();
        }

        private static Func<T, TKey> MakeKeySelector<T, TKey>(LambdaExpression selector)
        {
            var tParam = Expression.Parameter(typeof(T));
            var invokeExpr = Expression.Invoke(selector, tParam);

            return Expression.Lambda<Func<T, TKey>>(invokeExpr, tParam).Compile();
        }

        private void BuildDefaultSelector<TCompatibleRequest>(RequestConfig<TCompatibleRequest> config)
        {
            var kItem = config.GetRequestKeys();
            var kEntity = config.GetKeysFor<TEntity>();

            if (kItem != null && kItem.Length > 0 && kEntity != null && kItem.Length > 0)
            {
                if (kItem.Length != kEntity.Length)
                    throw new IncompatibleKeysException(typeof(TCompatibleRequest), typeof(TEntity));

                if (kItem.Length > 1)
                    throw new BadConfigurationException($"Composite keys are not supported for bulk requests");

                var selector = SelectorHelpers.BuildCollection<TRequest, TItem, TEntity>(
                    _getRequestItems, kEntity[0], kItem[0]);

                config.SetEntitySelector<TEntity>(selector);
            }
        }
    }
}
