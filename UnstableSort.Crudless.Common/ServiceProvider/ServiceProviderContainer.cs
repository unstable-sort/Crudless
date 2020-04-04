using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnstableSort.Crudless.Common.ServiceProvider
{
    public abstract class ServiceProviderContainer 
        : IServiceProvider, 
          IDisposable
    {
        IServiceProvider CurrentProvider { get; }

        public abstract void RegisterInstance(Type service, object instance);

        public abstract void RegisterInstance<TService>(TService instance)
            where TService : class;

        public abstract void Register(Type genericService, IEnumerable<Assembly> assemblies);

        public abstract void Register<TService>(Func<TService> factory)
            where TService : class;

        public abstract void RegisterScoped(Type service, Type implementation);

        public abstract void RegisterScoped<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        public abstract void RegisterSingleton(Type service, Func<object> factory);

        public abstract void RegisterSingleton(Type service, Type implementation);

        public abstract void RegisterSingleton<TService>(Func<TService> factory)
            where TService : class;

        public abstract void RegisterSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        public abstract void RegisterSingleton<TConcrete>()
            where TConcrete : class;

        public abstract void RegisterConditional(Type service,
            Type implementation,
            Predicate<ConditionalContext> predicate);

        public abstract void RegisterInitializer<TService>(Action<TService> initializer)
            where TService : class;

        public abstract void RegisterDecorator(Type service,
            Type decorator,
            Predicate<DecoratorConditionalContext> predicate);

        public abstract void RegisterDecorator(Type service,
            Func<DecoratorConditionalContext, Type> factory,
            Predicate<DecoratorConditionalContext> predicate);

        public abstract void RegisterDecorator(Type serviceType, Type decoratorType);

        public abstract void RegisterDecorator<TService, TDecorator>()
            where TService : class
            where TDecorator : class, TService;

        public abstract OverrideScope AllowOverrides();
        
        public abstract IServiceProvider CreateProvider();

        public abstract TService ProvideInstance<TService>() where TService : class;

        public abstract object ProvideInstance(Type type);

        public abstract void Dispose();

        protected OverrideScope CreateOverrideScope() => new OverrideScope(this);

        protected abstract void EndScope(OverrideScope scope);

        internal void EndOverrideScope(OverrideScope scope) => EndScope(scope);
    }
}
