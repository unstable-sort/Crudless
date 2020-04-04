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
        
        public TBuilder ConfigureOptions(Action<RequestOptionsConfig> config)
        {
            if (config == null)
            {
                OptionsConfig = null;
            }
            else
            {
                OptionsConfig = new RequestOptionsConfig();
                config(OptionsConfig);
            }

            return (TBuilder)this;
        }

        public TBuilder UseErrorHandlerFactory(Func<IErrorHandler> handlerFactory)
        {
            ErrorHandlerFactory = handlerFactory;

            return (TBuilder)this;
        }

        public TBuilder AddEntityHook<THook>()
            where THook : IEntityHook<TRequest, TEntity>
            => AddEntityHook<THook, TRequest, TEntity>();

        public TBuilder AddEntityHook<THook, TBaseRequest>()
            where THook : IEntityHook<TBaseRequest, TEntity>
            => AddEntityHook<THook, TBaseRequest, TEntity>();

        public TBuilder AddEntityHook<THook, TBaseRequest, TBaseEntity>()
            where TBaseEntity : class
            where THook : IEntityHook<TBaseRequest, TBaseEntity>
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddEntityHook), typeof(TBaseRequest), typeof(TRequest));

            if (!typeof(TBaseEntity).IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(AddEntityHook), typeof(TBaseEntity), typeof(TEntity));

            EntityHooks.Add(TypeEntityHookFactory.From<THook, TBaseRequest, TBaseEntity>());

            return (TBuilder)this;
        }

        public TBuilder AddEntityHook(Type hookType)
        {
            var addHookFn = GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(x => x.Name == "AddEntityHook" && x.IsGenericMethodDefinition && x.GetGenericArguments().Length == 3)
                .MakeGenericMethod(hookType, typeof(TRequest), typeof(TEntity));

            return (TBuilder)addHookFn.Invoke(this, null);
        }

        public TBuilder AddEntityHook<TBaseRequest, TBaseEntity>(IEntityHook<TBaseRequest, TBaseEntity> hook)
            where TBaseEntity : class
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddEntityHook), typeof(TBaseRequest), typeof(TRequest));

            if (!typeof(TBaseEntity).IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(AddEntityHook), typeof(TBaseEntity), typeof(TEntity));

            EntityHooks.Add(InstanceEntityHookFactory.From(hook));

            return (TBuilder)this;
        }

        public TBuilder AddEntityHook(Func<TRequest, TEntity, CancellationToken, Task> hook)
        {
            EntityHooks.Add(FunctionEntityHookFactory.From(hook));

            return (TBuilder)this;
        }

        public TBuilder AddEntityHook(Func<TRequest, TEntity, Task> hook)
            => AddEntityHook((request, entity, ct) => hook(request, entity));

        public TBuilder AddEntityHook(Action<TRequest, TEntity> hook)
        {
            EntityHooks.Add(FunctionEntityHookFactory.From(hook));

            return (TBuilder)this;
        }

        public TBuilder AddAuditHook<THook>()
            where THook : IAuditHook<TRequest, TEntity>
            => AddAuditHook<THook, TRequest, TEntity>();

        public TBuilder AddAuditHook<THook, TBaseRequest>()
            where THook : IAuditHook<TBaseRequest, TEntity>
            => AddAuditHook<THook, TBaseRequest, TEntity>();

        public TBuilder AddAuditHook<THook, TBaseRequest, TBaseEntity>()
            where TBaseEntity : class
            where THook : IAuditHook<TBaseRequest, TBaseEntity>
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddAuditHook), typeof(TBaseRequest), typeof(TRequest));

            if (!typeof(TBaseEntity).IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(AddAuditHook), typeof(TBaseEntity), typeof(TEntity));

            AuditHooks.Add(TypeAuditHookFactory.From<THook, TBaseRequest, TBaseEntity>());

            return (TBuilder)this;
        }

        public TBuilder AddAuditHook(Type hookType)
        {
            var addHookFn = GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(x => x.Name == "AddAuditHook" && x.IsGenericMethodDefinition && x.GetGenericArguments().Length == 3)
                .MakeGenericMethod(hookType, typeof(TRequest), typeof(TEntity));

            return (TBuilder)addHookFn.Invoke(this, null);
        }

        public TBuilder AddAuditHook<TBaseRequest, TBaseEntity>(IAuditHook<TBaseRequest, TBaseEntity> hook)
            where TBaseEntity : class
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddAuditHook), typeof(TBaseRequest), typeof(TRequest));

            if (!typeof(TBaseEntity).IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(AddAuditHook), typeof(TBaseEntity), typeof(TEntity));

            AuditHooks.Add(InstanceAuditHookFactory.From(hook));

            return (TBuilder)this;
        }

        public TBuilder AddAuditHook(Func<TRequest, TEntity, TEntity, CancellationToken, Task> hook)
        {
            AuditHooks.Add(FunctionAuditHookFactory.From(hook));

            return (TBuilder)this;
        }

        public TBuilder AddAuditHook(Func<TRequest, TEntity, TEntity, Task> hook)
            => AddAuditHook((request, oldEntity, newEntity, ct) => hook(request, oldEntity, newEntity));

        public TBuilder AddAuditHook(Action<TRequest, TEntity, TEntity> hook)
        {
            AuditHooks.Add(FunctionAuditHookFactory.From(hook));

            return (TBuilder)this;
        }

        public TBuilder UseEntityKey<TKey>(Expression<Func<TEntity, TKey>> entityKeyExpr)
        {
            EntityKeys = Key.MakeKeys(entityKeyExpr);

            return (TBuilder)this;
        }
        
        public TBuilder UseEntityKey(string entityKeyMember)
        {
            EntityKeys = new[] { Key.MakeKey<TEntity>(entityKeyMember) };

            return (TBuilder)this;
        }

        public TBuilder UseEntityKey(string[] entityKeyMembers)
        {
            EntityKeys = entityKeyMembers.Select(Key.MakeKey<TEntity>).ToArray();

            return (TBuilder)this;
        }
        
        public TBuilder WithDefault(TEntity defaultValue)
        {
            DefaultValue = defaultValue;

            return (TBuilder)this;
        }

        public TBuilder FilterWith<TFilter, TBaseRequest, TBaseEntity>()
            where TBaseEntity : class
            where TFilter : IFilter<TBaseRequest, TBaseEntity>
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(FilterWith), typeof(TBaseRequest), typeof(TRequest));

            if (!typeof(TBaseEntity).IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(FilterWith), typeof(TBaseEntity), typeof(TEntity));

            return AddRequestFilter(TypeFilterFactory.From<TFilter, TBaseRequest, TBaseEntity>());
        }

        public TBuilder FilterWith<TFilter, TBaseRequest>()
            where TFilter : IFilter<TBaseRequest, TEntity>
            => FilterWith<TFilter, TBaseRequest, TEntity>();

        public TBuilder FilterWith<TFilter>()
            where TFilter : IFilter<TRequest, TEntity>
            => FilterWith<TFilter, TRequest, TEntity>();

        public TBuilder FilterWith<TBaseRequest, TBaseEntity>(IFilter<TBaseRequest, TBaseEntity> filter)
            where TBaseEntity : class
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(FilterWith), typeof(TBaseRequest), typeof(TRequest));

            if (!typeof(TBaseEntity).IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(FilterWith), typeof(TBaseEntity), typeof(TEntity));

            return AddRequestFilter(InstanceFilterFactory.From(filter));
        }

        public TBuilder FilterWith<TBaseRequest>(
            Func<TBaseRequest, IQueryable<TEntity>, IQueryable<TEntity>> filterFunc)
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(FilterWith), typeof(TBaseRequest), typeof(TRequest));

            return AddRequestFilter(FunctionFilterFactory.From(filterFunc));
        }

        public TBuilder FilterWith(
            Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>> filterFunc)
            => FilterWith<TRequest>(filterFunc);

        public TBuilder SortWith(
            Action<SortBuilder<TRequest, TEntity>> build)
        {
            var builder = new SortBuilder<TRequest, TEntity>();
            build(builder);

            Sorter = builder.Build();
            
            return (TBuilder)this;
        }
        
        public TBuilder SortWith<TSorter, TBaseRequest>()
            where TSorter : ISorter<TBaseRequest, TEntity>
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(SortWith), typeof(TBaseRequest), typeof(TRequest));
            
            Sorter = TypeSorterFactory.From<TSorter, TBaseRequest, TEntity>();

            return (TBuilder)this;
        }
        
        public TBuilder SortWith<TSorter>()
            where TSorter : ISorter<TRequest, TEntity>
            => SortWith<TSorter, TRequest>();

        public TBuilder SortWith<TBaseRequest>(ISorter<TBaseRequest, TEntity> sorter)
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(SortWith), typeof(TBaseRequest), typeof(TRequest));

            Sorter = InstanceSorterFactory.From(sorter);

            return (TBuilder)this;
        }

        public TBuilder SortUsing<TBaseRequest>(
            Func<TBaseRequest, IQueryable<TEntity>, IOrderedQueryable<TEntity>> sortFunc)
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(SortUsing), typeof(TBaseRequest), typeof(TRequest));

            Sorter = FunctionSorterFactory.From(sortFunc);

            return (TBuilder)this;
        }

        public TBuilder SortUsing(Func<TRequest, IQueryable<TEntity>, IOrderedQueryable<TEntity>> sortFunc)
            => SortUsing<TRequest>(sortFunc);

        public TBuilder CreateResultWith<TResult>(
            Func<RequestContext<TRequest>, TEntity, CancellationToken, Task<TResult>> creator)
        {
            CreateResult = (context, entity, ct) => 
                creator(context.Cast<TRequest>(), entity, ct).ContinueWith(t => (object)t.Result);

            return (TBuilder)this;
        }

        public TBuilder CreateResultWith<TResult>(Func<RequestContext<TRequest>, TEntity, Task<TResult>> creator)
            => CreateResultWith((context, entity, ct) => creator(context, entity));

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

        /////////////////////////////////////////////////////////////

        internal TBuilder SetSelector(ISelector selector)
        {
            Selector = selector;
            return (TBuilder)this;
        }

        /////////////////////////////////////////////////////////////

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
        
        private TBuilder AddRequestFilter(IFilterFactory filter)
        {
            if (filter != null)
                _filters.Add(filter);

            return (TBuilder)this;
        }
    }
}
