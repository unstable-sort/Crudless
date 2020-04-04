using SimpleInjector;
using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.ServiceProvider.SimpleInjector
{
    public class SimpleInjectorServiceProvider : IServiceProvider
    {
        private SimpleInjectorServiceProviderContainer _container;
        private bool _ownsScope;

        public SimpleInjectorServiceProvider(SimpleInjectorServiceProviderContainer container, Scope scope, bool ownsScope)
        {
            _container = container;
            _ownsScope = ownsScope;
            Scope = scope;
        }

        public Scope Scope { get; private set; }

        public TService ProvideInstance<TService>() 
            where TService : class
        {
            try
            {
                return Scope.GetInstance<TService>();
            }
            catch (ActivationException exception)
            {
                throw new FailedToCreateServiceException("Failed to create service.", exception);
            }
        }

        public object ProvideInstance(System.Type service)
        {
            try
            {
                return Scope.GetInstance(service);
            }
            catch (ActivationException exception)
            {
                throw new FailedToCreateServiceException("Failed to create service.", exception);
            }
        }

        public void Dispose()
        {
            if (_ownsScope)
            {
                Scope?.Dispose();
                _ownsScope = false;
            }

            Scope = null;
            _container = null;
        }
    }
}
