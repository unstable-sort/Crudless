using System.Diagnostics.CodeAnalysis;
using SimpleInjector;
using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.Integration.SimpleInjector
{
    public class SimpleInjectorServiceProvider : IServiceProvider
    {
        private static int ScopeId = 0;

        private SimpleInjectorServiceProviderContainer _container;
        private bool _ownsScope;

        #if DEBUG
        [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "For debug use")]
        private int _scopeId;
        #endif

        public SimpleInjectorServiceProvider(SimpleInjectorServiceProviderContainer container, Scope scope, bool ownsScope)
        {
            _container = container;
            _ownsScope = ownsScope;
            Scope = scope;

            #if DEBUG
            _scopeId = ownsScope ? ++ScopeId : ScopeId;
            #endif
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

                --ScopeId;
            }

            Scope = null;
            _container = null;

            #if DEBUG
            _scopeId = 0;
            #endif
        }
    }
}
