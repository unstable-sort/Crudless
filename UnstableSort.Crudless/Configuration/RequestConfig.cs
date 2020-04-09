using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using UnstableSort.Crudless.Exceptions;
using UnstableSort.Crudless.Requests;

using IServiceProvider = UnstableSort.Crudless.Common.ServiceProvider.IServiceProvider;

namespace UnstableSort.Crudless.Configuration
{
    public interface IRequestConfig
    {
        Type RequestType { get; }

        ErrorConfig ErrorConfig { get; }

        RequestOptions GetOptionsFor<TEntity>()
            where TEntity : class;

        IKey[] GetRequestKeys();

        IKey[] GetKeysFor<TEntity>()
            where TEntity : class;

        ISelector GetSelectorFor<TEntity>()
            where TEntity : class;

        IBoxedSorter GetSorterFor<TEntity>(IServiceProvider provider)
            where TEntity : class;

        IRequestItemSource GetRequestItemSourceFor<TEntity>()
            where TEntity : class;

        TEntity GetDefaultFor<TEntity>()
            where TEntity : class;

        List<IBoxedFilter> GetFiltersFor<TEntity>(IServiceProvider provider)
            where TEntity : class;

        List<IBoxedRequestHook> GetRequestHooks(IServiceProvider provider);

        List<IBoxedEntityHook> GetEntityHooksFor<TEntity>(IServiceProvider provider)
            where TEntity : class;

        List<IBoxedItemHook> GetItemHooksFor<TEntity>(IServiceProvider provider)
            where TEntity : class;

        List<IBoxedAuditHook> GetAuditHooksFor<TEntity>(IServiceProvider provider)
            where TEntity : class;

        List<IBoxedResultHook> GetResultHooks(IServiceProvider provider);

        Func<BoxedRequestContext, object, CancellationToken, Task<TEntity>> GetCreatorFor<TEntity>()
            where TEntity : class;

        Func<BoxedRequestContext, object, TEntity, CancellationToken, Task<TEntity>> GetUpdatorFor<TEntity>()
            where TEntity : class;

        Func<BoxedRequestContext, TEntity, CancellationToken, Task<TResult>> GetResultCreatorFor<TEntity, TResult>()
            where TEntity : class;

        IEnumerable<Tuple<object, TEntity>> Join<TEntity>(IEnumerable<object> items, IEnumerable<TEntity> entities)
            where TEntity : class;
    }

    public class RequestConfig<TRequest>
        : IRequestConfig
    {
        private IKey[] _requestKeys;

        private readonly RequestHookConfig _requestHooks = new RequestHookConfig();

        private readonly Dictionary<Type, EntityHookConfig> _entityHooks
            = new Dictionary<Type, EntityHookConfig>();

        private readonly Dictionary<Type, ItemHookConfig> _itemHooks
            = new Dictionary<Type, ItemHookConfig>();

        private readonly Dictionary<Type, AuditHookConfig> _auditHooks
            = new Dictionary<Type, AuditHookConfig>();

        private readonly ResultHookConfig _resultHooks = new ResultHookConfig();

        private readonly RequestOptions _options = new RequestOptions();

        private readonly Dictionary<Type, RequestOptionsConfig> _entityOptionOverrides
            = new Dictionary<Type, RequestOptionsConfig>();

        private readonly Dictionary<Type, RequestOptions> _optionsCache
            = new Dictionary<Type, RequestOptions>();

        private readonly Dictionary<Type, Func<IEnumerable<object>, IEnumerable<object>, IEnumerable<Tuple<object, object>>>> _entityJoiners
            = new Dictionary<Type, Func<IEnumerable<object>, IEnumerable<object>, IEnumerable<Tuple<object, object>>>>();

        private readonly Dictionary<Type, IRequestItemSource> _entityRequestItemSources
            = new Dictionary<Type, IRequestItemSource>();

        private readonly Dictionary<Type, IKey[]> _entityKeys
            = new Dictionary<Type, IKey[]>();

        private readonly Dictionary<Type, ISorterFactory> _entitySorters
            = new Dictionary<Type, ISorterFactory>();

        private readonly Dictionary<Type, ISelector> _entitySelectors
            = new Dictionary<Type, ISelector>();

        private readonly Dictionary<Type, Func<BoxedRequestContext, object, CancellationToken, Task<object>>> _entityCreators
            = new Dictionary<Type, Func<BoxedRequestContext, object, CancellationToken, Task<object>>>();

        private readonly Dictionary<Type, Func<BoxedRequestContext, object, object, CancellationToken, Task<object>>> _entityUpdators
            = new Dictionary<Type, Func<BoxedRequestContext, object, object, CancellationToken, Task<object>>>();

        private readonly Dictionary<Type, Func<BoxedRequestContext, object, CancellationToken, Task<object>>> _entityResultCreators
            = new Dictionary<Type, Func<BoxedRequestContext, object, CancellationToken, Task<object>>>();

        private readonly Dictionary<Type, object> _defaultValues
            = new Dictionary<Type, object>();

        private readonly Dictionary<Type, FilterConfig> _entityFilters
            = new Dictionary<Type, FilterConfig>();

        public Type RequestType => typeof(TRequest);

        public ErrorConfig ErrorConfig { get; private set; } = new ErrorConfig();

        public RequestOptions GetOptionsFor<TEntity>()
            where TEntity : class
        {
            if (_optionsCache.TryGetValue(typeof(TEntity), out var cachedOptions))
                return cachedOptions;

            var options = _options.Clone();
            OverrideOptions(options, typeof(TEntity));

            _optionsCache[typeof(TEntity)] = options;

            return options;
        }

        public List<IBoxedRequestHook> GetRequestHooks(IServiceProvider provider)
        {
            return _requestHooks.GetHooks(provider);
        }

        public List<IBoxedEntityHook> GetEntityHooksFor<TEntity>(IServiceProvider provider)
            where TEntity : class
        {
            var hooks = new List<IBoxedEntityHook>();

            foreach (var type in typeof(TEntity).BuildTypeHierarchyDown())
            {
                if (_entityHooks.TryGetValue(type, out var entityHooks))
                    hooks.AddRange(entityHooks.GetHooks(provider));
            }

            return hooks;
        }

        public List<IBoxedItemHook> GetItemHooksFor<TEntity>(IServiceProvider provider)
            where TEntity : class
        {
            var hooks = new List<IBoxedItemHook>();

            foreach (var type in typeof(TEntity).BuildTypeHierarchyDown())
            {
                if (_itemHooks.TryGetValue(type, out var itemHooks))
                    hooks.AddRange(itemHooks.GetHooks(provider));
            }

            return hooks;
        }

        public List<IBoxedAuditHook> GetAuditHooksFor<TEntity>(IServiceProvider provider)
            where TEntity : class
        {
            var hooks = new List<IBoxedAuditHook>();

            foreach (var type in typeof(TEntity).BuildTypeHierarchyDown())
            {
                if (_auditHooks.TryGetValue(type, out var auditHooks))
                    hooks.AddRange(auditHooks.GetHooks(provider));
            }

            return hooks;
        }

        public List<IBoxedResultHook> GetResultHooks(IServiceProvider provider)
        {
            return _resultHooks.GetHooks(provider);
        }

        public IRequestItemSource GetRequestItemSourceFor<TEntity>()
            where TEntity : class
        {
            foreach (var type in typeof(TEntity).BuildTypeHierarchyUp())
            {
                if (_entityRequestItemSources.TryGetValue(type, out var itemSource))
                    return itemSource;
            }

            var source = RequestItemSource.From<TRequest, TRequest>(request => request);
            _entityRequestItemSources[typeof(TEntity)] = source;

            return source;
        }

        public IKey[] GetRequestKeys() => _requestKeys;
        
        public IKey[] GetKeysFor<TEntity>()
            where TEntity : class
        {
            foreach (var type in typeof(TEntity).BuildTypeHierarchyUp())
            {
                if (_entityKeys.TryGetValue(type, out var keys))
                    return keys;
            }

            return null;
        }

        public ISelector GetSelectorFor<TEntity>()
            where TEntity : class
        {
            foreach (var type in typeof(TEntity).BuildTypeHierarchyUp())
            {
                if (_entitySelectors.TryGetValue(type, out var selector))
                    return selector;
            }

            throw new BadConfigurationException(
                $"No selector defined for entity '{typeof(TEntity)}' " +
                $"for request '{typeof(TRequest)}'.");
        }
        
        public IBoxedSorter GetSorterFor<TEntity>(IServiceProvider provider)
            where TEntity : class
        {
            foreach (var type in typeof(TEntity).BuildTypeHierarchyUp())
            {
                if (_entitySorters.TryGetValue(type, out var sorter))
                    return sorter.Create(provider);
            }

            return null;
        }

        public List<IBoxedFilter> GetFiltersFor<TEntity>(IServiceProvider provider)
            where TEntity : class
        {
            var filters = new List<IBoxedFilter>();

            foreach (var type in typeof(TEntity).BuildTypeHierarchyDown())
            {
                if (_entityFilters.TryGetValue(type, out var entityFilters))
                    filters.AddRange(entityFilters.GetFilters(provider));
            }
            
            return filters;
        }

        public Func<BoxedRequestContext, object, CancellationToken, Task<TEntity>> GetCreatorFor<TEntity>()
            where TEntity : class
        {
            if (_entityCreators.TryGetValue(typeof(TEntity), out var creator))
                return (context, item, ct) 
                    => creator(context, item, ct).ContinueWith(t => (TEntity) t.Result);

            return (context, item, ct) => Task.FromResult(GetMapperFromContext(context).Map<TEntity>(item));
        }

        public Func<BoxedRequestContext, object, TEntity, CancellationToken, Task<TEntity>> GetUpdatorFor<TEntity>()
            where TEntity : class
        {
            if (_entityUpdators.TryGetValue(typeof(TEntity), out var updator))
                return (context, item, entity, ct) 
                    => updator(context, item, entity, ct).ContinueWith(t => (TEntity)t.Result);

            return (context, item, entity, ct) => Task.FromResult(GetMapperFromContext(context).Map(item, entity));
        }

        public Func<BoxedRequestContext, TEntity, CancellationToken, Task<TResult>> GetResultCreatorFor<TEntity, TResult>()
            where TEntity : class
        {
            if (_entityResultCreators.TryGetValue(typeof(TEntity), out var creator))
                return (context, entity, ct) => creator(context, entity, ct).ContinueWith(t => (TResult)t.Result);

            return (context, entity, ct) => Task.FromResult(GetMapperFromContext(context).Map<TResult>(entity));
        }
        
        public IEnumerable<Tuple<object, TEntity>> Join<TEntity>(
            IEnumerable<object> items, 
            IEnumerable<TEntity> entities)
            where TEntity : class
        {
            if (!_entityJoiners.TryGetValue(typeof(TEntity), out var joiner))
            {
                var message =
                    $"Unable to join entities of type '{entities.GetType()}' with request items of type '{items.GetType()}'.\r\n" +
                    "This is usually because no key has been defined for the entity or request.";

                throw new BadConfigurationException(message);
            }

            return joiner(items, entities).Select(t => new Tuple<object, TEntity>(t.Item1, (TEntity)t.Item2));
        }

        public TEntity GetDefaultFor<TEntity>()
            where TEntity : class
        {
            if (_defaultValues.TryGetValue(typeof(TEntity), out var entity))
                return (TEntity) entity;

            return null;
        }

        internal void SetOptions(RequestOptionsConfig options)
        {
            if (options != null)
                OverrideOptions(_options, options);
        }

        internal void SetOptionsFor<TEntity>(RequestOptionsConfig options)
        {
            if (_entityOptionOverrides.TryGetValue(typeof(TEntity), out var config))
                MergeOptions(options, config);
            else
                _entityOptionOverrides[typeof(TEntity)] = options;
        }

        internal void AddRequestHooks(List<IRequestHookFactory> hooks)
        {
            _requestHooks.AddHooks(hooks);
        }

        internal void AddEntityHooksFor<TEntity>(List<IEntityHookFactory> hooks)
            where TEntity : class
        {
            if (_entityHooks.TryGetValue(typeof(TEntity), out var hookConfig))
            {
                hookConfig.AddHooks(hooks);
            }
            else
            {
                var config = new EntityHookConfig();
                config.SetHooks(hooks);

                _entityHooks[typeof(TEntity)] = config;
            }
        }

        internal void AddItemHooksFor<TEntity>(List<IItemHookFactory> hooks)
        {
            if (_itemHooks.TryGetValue(typeof(TEntity), out var hookConfig))
            {
                hookConfig.AddHooks(hooks);
            }
            else
            {
                var config = new ItemHookConfig();
                config.SetHooks(hooks);

                _itemHooks[typeof(TEntity)] = config;
            }
        }

        internal void AddAuditHooksFor<TEntity>(List<IAuditHookFactory> hooks)
            where TEntity : class
        {
            if (_auditHooks.TryGetValue(typeof(TEntity), out var hookConfig))
            {
                hookConfig.AddHooks(hooks);
            }
            else
            {
                var config = new AuditHookConfig();
                config.SetHooks(hooks);

                _auditHooks[typeof(TEntity)] = config;
            }
        }

        internal void AddResultHooks(List<IResultHookFactory> hooks)
        {
            _resultHooks.AddHooks(hooks);
        }

        internal void AddEntityFiltersFor<TEntity>(List<IFilterFactory> filters)
        {
            if (_entityFilters.TryGetValue(typeof(TEntity), out var filterConfig))
            {
                filterConfig.AddFilters(filters);
            }
            else
            {
                var config = new FilterConfig();
                config.SetFilters(filters);

                _entityFilters[typeof(TEntity)] = config;
            }
        }

        internal void SetEntityRequestItemSource<TEntity>(IRequestItemSource itemSource)
            where TEntity : class
        {
            _entityRequestItemSources[typeof(TEntity)] = itemSource;
        }

        internal void SetRequestKeys(IKey[] keys)
        {
            _requestKeys = keys;
        }

        internal void SetEntityKeys<TEntity>(IKey[] keys)
            where TEntity : class
        {
            _entityKeys[typeof(TEntity)] = keys;
        }

        internal void SetEntitySelector<TEntity>(ISelector selector)
            where TEntity : class
        {
            _entitySelectors[typeof(TEntity)] = selector;
        }

        internal void SetEntitySorter<TEntity>(ISorterFactory sorter)
            where TEntity : class
        {
            _entitySorters[typeof(TEntity)] = sorter;
        }

        internal void SetEntityCreator<TEntity>(
            Func<BoxedRequestContext, object, CancellationToken, Task<TEntity>> creator)
            where TEntity : class
        {
            _entityCreators[typeof(TEntity)] = (context, item, ct) => 
                creator(context, item, ct).ContinueWith(t => (object)t.Result);
        }

        internal void SetEntityUpdator<TEntity>(
            Func<BoxedRequestContext, object, TEntity, CancellationToken, Task<TEntity>> updator)
            where TEntity : class
        {
            _entityUpdators[typeof(TEntity)] = (context, item, entity, ct) => 
                updator(context, item, (TEntity)entity, ct).ContinueWith(t => (object)t.Result);
        }

        internal void SetEntityResultCreator<TEntity>(
            Func<BoxedRequestContext, TEntity, CancellationToken, Task<object>> creator)
            where TEntity : class
        {
            _entityResultCreators[typeof(TEntity)] = (context, entity, ct) => 
                creator(context, (TEntity)entity, ct);
        }

        internal void SetEntityJoiner<TEntity>(
            Func<IEnumerable<object>, IEnumerable<TEntity>, IEnumerable<Tuple<object, TEntity>>> joiner)
            where TEntity : class
        {
            _entityJoiners[typeof(TEntity)] = (x, y) => 
                joiner(x, y.Cast<TEntity>()).Select(t => new Tuple<object, object>(t.Item1, t.Item2));
        }

        internal void SetEntityDefault<TEntity>(
            TEntity defaultValue)
            where TEntity : class
        {
            _defaultValues[typeof(TEntity)] = defaultValue;
        }

        private void MergeOptions(RequestOptionsConfig newOptions, RequestOptionsConfig existingOptions)
        {
            if (newOptions.UseProjection.HasValue)
                existingOptions.UseProjection = newOptions.UseProjection;
        }

        private void OverrideOptions(RequestOptions options, Type tEntity)
        {
            foreach (var type in tEntity.BuildTypeHierarchyDown())
            {
                if (_entityOptionOverrides.TryGetValue(type, out var entityOptions))
                    OverrideOptions(options, entityOptions);
            }
        }

        private void OverrideOptions(RequestOptions options, RequestOptionsConfig config)
        {
            if (config.UseProjection.HasValue)
                options.UseProjection = config.UseProjection.Value;
        }

        private static IMapper GetMapperFromContext(BoxedRequestContext context)
            => context.Cast<TRequest>().ServiceProvider.ProvideInstance<IMapper>();
    }
}
