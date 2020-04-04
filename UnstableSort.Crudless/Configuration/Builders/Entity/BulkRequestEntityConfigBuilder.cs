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
        
        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> WithRequestItems(
            Expression<Func<TRequest, ICollection<TItem>>> requestItemsExpr)
        {
            _getRequestItems = requestItemsExpr;
            RequestItemSource = UnstableSort.Crudless.RequestItemSource.From(BuildItemSource(requestItemsExpr));

            return this;
        }

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> AddItemHook<THook>()
            where THook : IItemHook<TRequest, TItem>
            => AddItemHook<THook, TRequest>();

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> AddItemHook<TBaseRequest>(
            IItemHook<TBaseRequest, TItem> hook)
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddItemHook), typeof(TBaseRequest), typeof(TRequest));

            _itemHooks.Add(InstanceItemHookFactory.From(hook));

            return this;
        }

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> AddItemHook<THook, TBaseRequest>()
            where THook : IItemHook<TBaseRequest, TItem>
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddItemHook), typeof(TBaseRequest), typeof(TRequest));
            
            _itemHooks.Add(TypeItemHookFactory.From<THook, TBaseRequest, TItem>());

            return this;
        }

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> AddItemHook(Type hookType)
        {
            var addHookFn = GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(x => x.Name == nameof(AddItemHook) && 
                             x.IsGenericMethodDefinition && 
                             x.GetGenericArguments().Length == 2)
                .MakeGenericMethod(hookType, typeof(TRequest));

            return (BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>)addHookFn.Invoke(this, null);
        }

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> AddItemHook(
            Func<TRequest, TItem, CancellationToken, Task<TItem>> hook)
        {
            _itemHooks.Add(FunctionItemHookFactory.From(hook));

            return this;
        }

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> AddItemHook(
            Func<TRequest, TItem, Task<TItem>> hook)
            => AddItemHook((request, item, ct) => hook(request, item));

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> AddItemHook(
            Func<TRequest, TItem, TItem> hook)
        {
            _itemHooks.Add(FunctionItemHookFactory.From(hook));

            return this;
        }

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UseRequestItemKey<TKey>(
            Expression<Func<TItem, TKey>> itemKeyExpr)
        {
            RequestItemKeys = new[] { new Key(typeof(TKey), itemKeyExpr) };

            return this;
        }

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UseRequestItemKey(
            string itemKeyProperty)
        {
            RequestItemKeys = new[] { Key.MakeKey<TItem>(itemKeyProperty) };

            return this;
        }

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UseRequestItemKey(
            string[] itemKeyMembers)
        {
            RequestItemKeys = itemKeyMembers.Select(Key.MakeKey<TItem>).ToArray();

            return this;
        }

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> CreateEntityWith(
            Func<RequestContext<TRequest>, TItem, CancellationToken, Task<TEntity>> creator)
        {
            CreateEntity = (context, item, ct) => creator(context.Cast<TRequest>(), (TItem)item, ct);

            return this;
        }

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> CreateEntityWith(
            Func<RequestContext<TRequest>, TItem, Task<TEntity>> creator)
            => CreateEntityWith((context, item, ct) => creator(context, item));

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

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> CreateEntityWith(
            Func<TItem, CancellationToken, Task<TEntity>> creator)
        {
            CreateEntity = (context, item, ct) => creator((TItem)item, ct);

            return this;
        }

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> CreateEntityWith(
            Func<TItem, Task<TEntity>> creator)
            => CreateEntityWith((item, ct) => creator(item));

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

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UpdateEntityWith(
            Func<RequestContext<TRequest>, TItem, TEntity, CancellationToken, Task<TEntity>> updator)
        {
            UpdateEntity = (context, item, entity, ct) => updator(context.Cast<TRequest>(), (TItem)item, entity, ct);

            return this;
        }

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UpdateEntityWith(
            Func<RequestContext<TRequest>, TItem, TEntity, Task<TEntity>> updator)
            => UpdateEntityWith((context, item, entity, ct) => updator(context, item, entity));

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

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UpdateEntityWith(
            Func<TItem, TEntity, CancellationToken, Task<TEntity>> updator)
        {
            UpdateEntity = (context, item, entity, ct) => updator((TItem)item, entity, ct);

            return this;
        }

        public BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UpdateEntityWith(
            Func<TItem, TEntity, Task<TEntity>> updator)
            => UpdateEntityWith((item, entity, ct) => updator(item, entity));

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

        public override void Build<TCompatibleRequest>(RequestConfig<TCompatibleRequest> config)
        {
            if (_getRequestItems == null)
            {
                var message =
                    $"No request item source has been defined for '{typeof(TRequest)}'." +
                    $"Define item source by calling `{nameof(WithRequestItems)}` in the request's profile.";

                throw new BadConfigurationException(message);
            }

            base.Build(config);

            if (Selector == null)
                DefaultSelector(config);

            BuildJoiner(config);

            config.AddItemHooksFor<TEntity>(_itemHooks);
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

        private void DefaultSelector<TCompatibleRequest>(
            RequestConfig<TCompatibleRequest> config)
        {
            var itemKeys = config.GetRequestKeys();
            var entityKeys = config.GetKeysFor<TEntity>();

            if (itemKeys != null && itemKeys.Length > 0 &&
                entityKeys != null && itemKeys.Length > 0)
            {
                if (itemKeys.Length != entityKeys.Length)
                    throw new BadConfigurationException($"Incompatible keys defined for '{typeof(TCompatibleRequest)}' and '{typeof(TEntity)}'");

                if (itemKeys.Length > 1)
                    throw new BadConfigurationException($"Composite keys are not supported for bulk requests");

                var builder = new SelectorBuilder<TRequest, TEntity>();
                config.SetEntitySelector<TEntity>(builder.Collection(_getRequestItems, entityKeys[0], itemKeys[0]));
            }
        }

        private void BuildJoiner<TCompatibleRequest>(RequestConfig<TCompatibleRequest> config)
        {
            if (EntityKeys == null || EntityKeys.Length == 0 || 
                RequestItemKeys == null || RequestItemKeys.Length == 0)
                return;

            if (RequestItemKeys.Length != EntityKeys.Length)
                throw new BadConfigurationException($"Incompatible keys defined for '{typeof(TCompatibleRequest)}' and '{typeof(TEntity)}'");

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

        private static Func<T, TKey> MakeKeySelector<T, TKey>(LambdaExpression selector)
        {
            var tParam = Expression.Parameter(typeof(T));
            var invokeExpr = Expression.Invoke(selector, tParam);
            return Expression.Lambda<Func<T, TKey>>(invokeExpr, tParam).Compile();
        }
    }
}
