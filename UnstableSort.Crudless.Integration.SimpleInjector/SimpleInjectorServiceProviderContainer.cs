using System;
using System.Collections.Generic;
using System.Reflection;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using UnstableSort.Crudless.Common.ServiceProvider;

using IServiceProvider = UnstableSort.Crudless.Common.ServiceProvider.IServiceProvider;

namespace UnstableSort.Crudless.Integration.SimpleInjector
{
    public class SimpleInjectorServiceProviderContainer : ServiceProviderContainer, IDisposable
    {
        private Container _container;
        private int _overrideScopes = 0;
        
        public SimpleInjectorServiceProviderContainer(Container container)
        {
            _container = container;
        }
        
        public override object ProvideInstance(Type service)
        {
            try
            {
                return _container.GetInstance(service);
            }
            catch (ActivationException exception)
            {
                throw new FailedToCreateServiceException("Failed to create service.", exception);
            }
        }
        
        public override TService ProvideInstance<TService>()
        {
            try
            {
                return _container.GetInstance<TService>();
            }
            catch (ActivationException exception)
            {
                throw new FailedToCreateServiceException("Failed to create service.", exception);
            }
        }

        public override void RegisterInstance(Type service, object instance)
            => _container.RegisterInstance(service, instance);

        public override void RegisterInstance<TService>(TService instance)
            => _container.RegisterInstance(instance);

        public override void Register(Type genericService, IEnumerable<Assembly> assemblies)
            => _container.Register(genericService, assemblies);

        public override void Register(Type concreteType)
            => _container.Register(concreteType);

        public override void Register(Type service, Type implementation)
            => _container.Register(service, implementation);

        public override void Register<TService>(Func<TService> factory)
            => _container.Register(factory);

        public override void Register<TService, TImplementation>()
            => _container.Register<TService, TImplementation>();

        public override void RegisterScoped(Type service, Type implementation)
            => _container.Register(service, implementation, Lifestyle.Scoped);

        public override void RegisterScoped<TService, TImplementation>()
            => _container.Register<TService, TImplementation>(Lifestyle.Scoped);

        public override void RegisterSingleton(Type service, Func<object> factory)
            => _container.RegisterSingleton(service, factory);

        public override void RegisterSingleton(Type service, Type implementation)
            => _container.RegisterSingleton(service, implementation);

        public override void RegisterSingleton<TService>(Func<TService> factory)
            => _container.RegisterSingleton(factory);

        public override void RegisterSingleton<TConcrete>()
            => _container.RegisterSingleton<TConcrete>();

        public override void RegisterSingleton<TService, TImplementation>()
            => _container.RegisterSingleton<TService, TImplementation>();

        public override void RegisterConditional(Type service, Type implementation, Predicate<ConditionalContext> predicate)
            => _container.RegisterConditional(service, implementation, context => predicate(context.ToConditionalContext()));

        public override void RegisterInitializer<TService>(Action<TService> initializer)
            => _container.RegisterInitializer(initializer);

        public override void RegisterDecorator<TService, TDecorator>()
            => _container.RegisterDecorator<TService, TDecorator>();

        public override void RegisterDecorator(Type service, Type decorator, Predicate<DecoratorConditionalContext> predicate)
            => _container.RegisterDecorator(service, decorator, context => predicate(context.ToConditionalContext()));

        public override void RegisterDecorator(Type service, Func<DecoratorConditionalContext, Type> factory, Predicate<DecoratorConditionalContext> predicate)
            => _container.RegisterDecorator(service,
                context => factory(context.ToConditionalContext()),
                Lifestyle.Transient,
                context => predicate(context.ToConditionalContext()));

        public override void RegisterDecorator(Type serviceType, Type decoratorType)
            => _container.RegisterDecorator(serviceType, decoratorType);

        public override OverrideScope AllowOverrides()
        {
            if (++_overrideScopes == 1)
                _container.Options.AllowOverridingRegistrations = true;

            return CreateOverrideScope();
        }
        
        public override IServiceProvider CreateProvider()
        {
            return new SimpleInjectorServiceProvider(this, AsyncScopedLifestyle.BeginScope(_container), true);
        }

        public override IServiceProvider GetProvider()
        {
            var scope = Lifestyle.Scoped.GetCurrentScope(_container);

            if (scope != null)
                return new SimpleInjectorServiceProvider(this, scope, false);
            else
                return new SimpleInjectorServiceProvider(this, AsyncScopedLifestyle.BeginScope(_container), true);
        }

        public override void Dispose()
        {
            _container = null;
        }

        protected override void EndScope(OverrideScope scope)
        {
            if (--_overrideScopes <= 0)
            {
                _container.Options.AllowOverridingRegistrations = false;
                _overrideScopes = 0;
            }
        }
    }
}
