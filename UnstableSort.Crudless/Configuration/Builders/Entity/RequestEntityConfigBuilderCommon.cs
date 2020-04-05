using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration.Builders.Sort;
using UnstableSort.Crudless.Errors;
using UnstableSort.Crudless.Exceptions;
using UnstableSort.Crudless.Requests;

// ReSharper disable once CheckNamespace
namespace UnstableSort.Crudless.Configuration.Builders
{
    public abstract class RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        : IRequestEntityConfigBuilder
        where TEntity : class
        where TBuilder : RequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
    {
        private readonly List<IFilterFactory> _filters = new List<IFilterFactory>();

        protected readonly List<IEntityHookFactory> EntityHooks
            = new List<IEntityHookFactory>();

        protected readonly List<IAuditHookFactory> AuditHooks
            = new List<IAuditHookFactory>();

        protected RequestOptionsConfig OptionsConfig;
        protected TEntity DefaultValue;
        protected ISorterFactory Sorter;
        protected ISelector Selector;
        protected IRequestItemSource RequestItemSource;
        protected Key[] EntityKeys;
        protected Key[] RequestItemKeys;
        protected Func<BoxedRequestContext, object, CancellationToken, Task<TEntity>> CreateEntity;
        protected Func<BoxedRequestContext, object, TEntity, CancellationToken, Task<TEntity>> UpdateEntity;
        protected Func<BoxedRequestContext, TEntity, CancellationToken, Task<object>> CreateResult;
        protected Func<IErrorHandler> ErrorHandlerFactory;

        public virtual void Build<TCompatibleRequest>(RequestConfig<TCompatibleRequest> config)
        {
            if (OptionsConfig != null)
                config.SetOptionsFor<TEntity>(OptionsConfig);

            if (ErrorHandlerFactory != null)
                config.ErrorConfig.SetErrorHandlerFor(typeof(TEntity), ErrorHandlerFactory);

            config.SetEntityDefault(DefaultValue);

            if (RequestItemKeys != null && RequestItemKeys.Length > 0)
                config.SetRequestKeys(RequestItemKeys);

            if (EntityKeys != null && EntityKeys.Length > 0)
                config.SetEntityKeys<TEntity>(EntityKeys);

            if (RequestItemSource != null)
                config.SetEntityRequestItemSource<TEntity>(RequestItemSource);

            if (Selector != null)
                config.SetEntitySelector<TEntity>(Selector);

            if (CreateEntity != null)
                config.SetEntityCreator(CreateEntity);

            if (UpdateEntity != null)
                config.SetEntityUpdator(UpdateEntity);

            if (CreateResult != null)
                config.SetEntityResultCreator(CreateResult);

            if (Sorter != null)
                config.SetEntitySorter<TEntity>(Sorter);

            if (_filters.Count > 0)
                config.AddEntityFiltersFor<TEntity>(_filters);

            config.AddEntityHooksFor<TEntity>(EntityHooks);
            config.AddAuditHooksFor<TEntity>(AuditHooks);
        }

        /// <summary>
        /// Configures additional request handler options when operating on this entity type.
        /// </summary>
        public TBuilder UseOptions(Action<RequestOptionsConfig> config)
        {
            if (config == null)
            {
                OptionsConfig = null;
            }
            else
            {
                if (OptionsConfig == null)
                    OptionsConfig = new RequestOptionsConfig();

                config(OptionsConfig);
            }

            return (TBuilder)this;
        }

        /// <summary>
        /// Sets the default error handler for the request type when operating on this entity type.
        /// The global error handler will be used if this has not been set.
        /// </summary>
        public TBuilder UseErrorHandlerFactory(Func<IErrorHandler> handlerFactory)
        {
            ErrorHandlerFactory = handlerFactory;

            return (TBuilder)this;
        }

        /// <summary>
        /// Sets the default value that should be returned from "get" requests when they have failed to find an entity.
        /// </summary>
        public TBuilder UseDefaultValue(TEntity defaultValue)
        {
            DefaultValue = defaultValue;

            return (TBuilder)this;
        }

        /// <summary>
        /// Provides an entity's "key" members.
        /// </summary>
        public TBuilder UseEntityKey<TKey>(Expression<Func<TEntity, TKey>> entityKeys)
        {
            EntityKeys = Key.MakeKeys(entityKeys);

            return (TBuilder)this;
        }

        /// <summary>
        /// Provides an entity's "key" member.
        /// </summary>
        public TBuilder UseEntityKey(string entityKeyMember)
        {
            EntityKeys = new[] { Key.MakeKey<TEntity>(entityKeyMember) };

            return (TBuilder)this;
        }

        /// <summary>
        /// Provides an entity's "key" members.
        /// </summary>
        public TBuilder UseEntityKey(string[] entityKeyMembers)
        {
            EntityKeys = entityKeyMembers.Select(Key.MakeKey<TEntity>).ToArray();

            return (TBuilder)this;
        }

        /// <summary>
        /// Provides request handlers with how to create a result from an entity.
        /// The default method is to resolve an IMapper and map the entity into a new TResult.
        /// </summary>
        public TBuilder CreateResultWith<TResult>(
            Func<RequestContext<TRequest>, TEntity, CancellationToken, Task<TResult>> creator)
        {
            CreateResult = (context, entity, ct) =>
                creator(context.Cast<TRequest>(), entity, ct).ContinueWith(t => (object)t.Result);

            return (TBuilder)this;
        }

        /// <summary>
        /// Provides request handlers with how to create a result from an entity.
        /// The default method is to resolve an IMapper and map the entity into a new TResult.
        /// </summary>
        public TBuilder CreateResultWith<TResult>(Func<RequestContext<TRequest>, TEntity, Task<TResult>> creator)
            => CreateResultWith((context, entity, ct) => creator(context, entity));

        /// <summary>
        /// Provides request handlers with how to create a result from an entity.
        /// The default method is to resolve an IMapper and map the entity into a new TResult.
        /// </summary>
        public TBuilder CreateResultWith<TResult>(Func<RequestContext<TRequest>, TEntity, TResult> creator)
        {
            CreateResult = (context, entity, ct) =>
            {
                if (ct.IsCancellationRequested)
                    return Task.FromCanceled<object>(ct);

                return Task.FromResult((object)creator(context.Cast<TRequest>(), entity));
            };

            return (TBuilder)this;
        }

        /// <summary>
        /// Adds a request filter of the given type.
        /// The filter will be resolved through the service provider.
        /// </summary>
        public TBuilder AddFilter(Type filterType)
        {
            var baseFilterType = filterType
                .GetBaseTypes()
                .SingleOrDefault(x => 
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(Filter<,>));

            if (baseFilterType == null)
                throw new ArgumentException($"Unable to add '{filterType}' as a filter for '{typeof(TRequest)}'.\r\n" +
                                            $"Filters must inherit Filter<TRequest, TEntity>.");

            var requestType = baseFilterType.GenericTypeArguments[0];
            
            if (!requestType.IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddFilter), requestType, typeof(TRequest));

            var entityType = baseFilterType.GenericTypeArguments[1];
            if (!entityType.IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(AddFilter), entityType, typeof(TEntity));

            var factoryMethod = typeof(TypeFilterFactory)
                .GetMethod(nameof(TypeFilterFactory.From), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(filterType, requestType, entityType);

            try
            {
                return AddRequestFilter((IFilterFactory)factoryMethod.Invoke(null, Array.Empty<object>()));
            }
            catch(TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw e;
            }
        }

        /// <summary>
        /// Adds a request filter instance.
        /// </summary>
        public TBuilder AddFilter(IFilter filter)
        {
            var filterType = filter.GetType();

            var baseFilterType = filterType
                .GetBaseTypes()
                .SingleOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(Filter<,>));

            if (baseFilterType == null)
                throw new ArgumentException($"Unable to add '{filterType}' as a filter for '{typeof(TRequest)}'.\r\n" +
                                            $"Filters must inherit Filter<TRequest, TEntity>.");

            var requestType = baseFilterType.GenericTypeArguments[0];

            if (!requestType.IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddFilter), requestType, typeof(TRequest));

            var entityType = baseFilterType.GenericTypeArguments[1];
            if (!entityType.IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(AddFilter), entityType, typeof(TEntity));

            var factoryMethod = typeof(InstanceFilterFactory)
                .GetMethod(nameof(InstanceFilterFactory.From), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(requestType, entityType);

            try
            {
                return AddRequestFilter((IFilterFactory)factoryMethod.Invoke(null, new object[] { filter }));
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw e;
            }
        }

        /// <summary>
        /// Adds a request filter of the given type.
        /// The filter will be resolved through the service provider.
        /// </summary>
        public TBuilder AddFilter<TFilter>() 
            where TFilter : IFilter
                => AddFilter(typeof(TFilter));

        /// <summary>
        /// Configures how a query's results should be ordered.
        /// </summary>
        public TBuilder Sort(Action<BasicSortBuilder<TRequest, TEntity>> configure)
        {
            var builder = new BasicSortBuilder<TRequest, TEntity>();

            configure(builder);

            return SetSorter(builder.Build());
        }

        /// <summary>
        /// Configures how a query's results should be ordered.
        /// See the docs for more information on "Table Sorting".
        /// </summary>
        public TBuilder SortAsTable<TControl>(Action<TableSortBuilder<TRequest, TEntity, TControl>> configure)
        {
            var builder = new TableSortBuilder<TRequest, TEntity, TControl>();

            configure(builder);

            var sorterFactory = builder.Build();

            return SetSorter(sorterFactory);
        }

        /// <summary>
        /// Configures how a query's results should be ordered.
        /// See the docs for more information on "Variant Sorting".
        /// </summary>
        public TBuilder SortAsVariant<TSwitch>(
            string switchProperty,
            Action<SwitchSortBuilder<TRequest, TEntity, TSwitch>> configure)
            where TSwitch : class
        {
            var requestParam = Expression.Parameter(typeof(TRequest), "r");
            var requestProp = Expression.PropertyOrField(requestParam, switchProperty);
            var readPropExpr = Expression.Lambda<Func<TRequest, TSwitch>>(requestProp, requestParam);

            var builder = new SwitchSortBuilder<TRequest, TEntity, TSwitch>(readPropExpr.Compile());

            configure(builder);

            var sorterFactory = builder.Build();

            return SetSorter(sorterFactory);
        }

        /// <summary>
        /// Configures how a query's results should be ordered.
        /// The sorter will be resolved through the service provider.
        /// </summary>
        public TBuilder SortCustom(Type sorterType)
        {
            var baseSorterType = sorterType
                .GetBaseTypes()
                .SingleOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(Sorter<,>));

            if (baseSorterType == null)
                throw new ArgumentException($"Unable to set '{sorterType}' as the sorter for '{typeof(TRequest)}'.\r\n" +
                                            $"Sorters must inherit Sorter<TRequest, TEntity>.");

            var requestType = baseSorterType.GenericTypeArguments[0];

            if (!requestType.IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(SortCustom), requestType, typeof(TRequest));

            var entityType = baseSorterType.GenericTypeArguments[1];
            if (!entityType.IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(SortCustom), entityType, typeof(TEntity));

            var factoryMethod = typeof(TypeSorterFactory)
                .GetMethod(nameof(TypeSorterFactory.From), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(sorterType, requestType, entityType);

            try
            {
                return SetSorter((ISorterFactory)factoryMethod.Invoke(null, Array.Empty<object>()));
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw e;
            }
        }

        /// <summary>
        /// Configures how a query's results should be ordered.
        /// Sorting will be acheived by the provided sorter object.
        /// </summary>
        public TBuilder SortCustom(ISorter sorter)
        {
            var sorterType = sorter.GetType();

            var baseSorterType = sorterType
                .GetBaseTypes()
                .SingleOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(Sorter<,>));

            if (baseSorterType == null)
                throw new ArgumentException($"Unable to set '{sorterType}' as the sorter for '{typeof(TRequest)}'.\r\n" +
                                            $"Sorters must inherit Sorter<TRequest, TEntity>.");

            var requestType = baseSorterType.GenericTypeArguments[0];

            if (!requestType.IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(SortCustom), requestType, typeof(TRequest));

            var entityType = baseSorterType.GenericTypeArguments[1];
            if (!entityType.IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(SortCustom), entityType, typeof(TEntity));

            var factoryMethod = typeof(InstanceSorterFactory)
                .GetMethod(nameof(InstanceSorterFactory.From), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(requestType, entityType);

            try
            {
                return SetSorter((ISorterFactory)factoryMethod.Invoke(null, new object[] { sorter }));
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw e;
            }
        }

        /// <summary>
        /// Configures how a query's results should be ordered.
        /// The sorter will be resolved through the service provider.
        /// </summary>
        public TBuilder SortCustom<TSorter>()
            where TSorter : ISorter
                => SortCustom(typeof(TSorter));

        /// <summary>
        /// Configures how a query's results should be ordered.
        /// </summary>
        public TBuilder SortCustom(Func<TRequest, IQueryable<TEntity>, IOrderedQueryable<TEntity>> sortFunc)
        {
            return SetSorter(FunctionSorterFactory.From(sortFunc));
        }

        /// <summary>
        /// Adds an entity hook of the given type.
        /// The hook will be resolved through the service provider.
        /// </summary>
        public TBuilder AddEntityHook(Type hookType)
        {
            var baseHookType = hookType
                .GetBaseTypes()
                .SingleOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(EntityHook<,>));

            if (baseHookType == null)
                throw new ArgumentException($"Unable to add '{hookType}' as an entity hook for '{typeof(TRequest)}'.\r\n" +
                                            $"Entity hooks must inherit EntityHook<TRequest, TEntity>.");

            var requestType = baseHookType.GenericTypeArguments[0];
            if (!requestType.IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddEntityHook), requestType, typeof(TRequest));

            var entityType = baseHookType.GenericTypeArguments[1];
            if (!entityType.IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(AddEntityHook), entityType, typeof(TEntity));

            var factoryMethod = typeof(TypeEntityHookFactory)
                .GetMethod(nameof(TypeEntityHookFactory.From), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(hookType, requestType, entityType);

            try
            {
                return AddEntityHook((IEntityHookFactory)factoryMethod.Invoke(null, Array.Empty<object>()));
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw e;
            }
        }

        /// <summary>
        /// Adds an entity hook instance.
        /// </summary>
        public TBuilder AddEntityHook(IEntityHook hook)
        {
            var hookType = hook.GetType();

            var baseHookType = hookType
                .GetBaseTypes()
                .SingleOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(EntityHook<,>));

            if (baseHookType == null)
                throw new ArgumentException($"Unable to add '{hookType}' as an entity hook for '{typeof(TRequest)}'.\r\n" +
                                            $"Entity hooks must inherit EntityHook<TRequest, TEntity>.");

            var requestType = baseHookType.GenericTypeArguments[0];
            if (!requestType.IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddEntityHook), requestType, typeof(TRequest));

            var entityType = baseHookType.GenericTypeArguments[1];
            if (!entityType.IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(AddEntityHook), entityType, typeof(TEntity));

            var factoryMethod = typeof(InstanceEntityHookFactory)
                .GetMethod(nameof(InstanceEntityHookFactory.From), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(requestType, entityType);

            try
            {
                return AddEntityHook((IEntityHookFactory)factoryMethod.Invoke(null, new object[] { hook }));
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw e;
            }
        }

        /// <summary>
        /// Adds an entity hook of the given type.
        /// The hook will be resolved through the service provider.
        /// </summary>
        public TBuilder AddEntityHook<THook>()
            where THook : IEntityHook
                => AddEntityHook(typeof(THook));

        /// <summary>
        /// Adds an audit hook of the given type.
        /// The hook will be resolved through the service provider.
        /// </summary>
        public TBuilder AddAuditHook(Type hookType)
        {
            var baseHookType = hookType
                .GetBaseTypes()
                .SingleOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(AuditHook<,>));

            if (baseHookType == null)
                throw new ArgumentException($"Unable to add '{hookType}' as an audit hook for '{typeof(TRequest)}'.\r\n" +
                                            $"Audit hooks must inherit AuditHook<TRequest, TEntity>.");

            var requestType = baseHookType.GenericTypeArguments[0];
            if (!requestType.IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddAuditHook), requestType, typeof(TRequest));

            var entityType = baseHookType.GenericTypeArguments[1];
            if (!entityType.IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(AddAuditHook), entityType, typeof(TEntity));

            var factoryMethod = typeof(TypeAuditHookFactory)
                .GetMethod(nameof(TypeAuditHookFactory.From), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(hookType, requestType, entityType);

            try
            {
                return AddAuditHook((IAuditHookFactory)factoryMethod.Invoke(null, Array.Empty<object>()));
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw e;
            }
        }

        /// <summary>
        /// Adds an audit hook instance.
        /// </summary>
        public TBuilder AddAuditHook(IAuditHook hook)
        {
            var hookType = hook.GetType();

            var baseHookType = hookType
                .GetBaseTypes()
                .SingleOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(AuditHook<,>));

            if (baseHookType == null)
                throw new ArgumentException($"Unable to add '{hookType}' as an audit hook for '{typeof(TRequest)}'.\r\n" +
                                            $"Audit hooks must inherit AuditHook<TRequest, TEntity>.");

            var requestType = baseHookType.GenericTypeArguments[0];
            if (!requestType.IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddAuditHook), requestType, typeof(TRequest));

            var entityType = baseHookType.GenericTypeArguments[1];
            if (!entityType.IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(AddAuditHook), entityType, typeof(TEntity));

            var factoryMethod = typeof(InstanceAuditHookFactory)
                .GetMethod(nameof(InstanceAuditHookFactory.From), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(requestType, entityType);

            try
            {
                return AddAuditHook((IAuditHookFactory)factoryMethod.Invoke(null, new object[] { hook }));
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw e;
            }
        }

        /// <summary>
        /// Adds an audit hook of the given type.
        /// The hook will be resolved through the service provider.
        /// </summary>
        public TBuilder AddAuditHook<THook>()
            where THook : IAuditHook
                => AddAuditHook(typeof(THook));

        internal TBuilder SetSelector(ISelector selector)
        {
            Selector = selector;
            return (TBuilder)this;
        }

        internal TBuilder AddRequestFilter(Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>> filter)
        {
            return AddRequestFilter(FunctionFilterFactory.From(filter));
        }

        internal TBuilder AddRequestFilter(IFilterFactory filter)
        {
            if (filter != null)
                _filters.Add(filter);

            return (TBuilder)this;
        }

        internal TBuilder SetSorter(ISorterFactory sorterFactory)
        {
            Sorter = sorterFactory;
            return (TBuilder)this;
        }

        internal TBuilder AddEntityHook(IEntityHookFactory entityHookFactory)
        {
            EntityHooks.Add(entityHookFactory);
            return (TBuilder)this;
        }

        internal TBuilder AddAuditHook(IAuditHookFactory auditHookFactory)
        {
            AuditHooks.Add(auditHookFactory);
            return (TBuilder)this;
        }
    }
}
