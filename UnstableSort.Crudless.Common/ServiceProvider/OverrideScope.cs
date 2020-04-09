using System;

namespace UnstableSort.Crudless.Common.ServiceProvider
{
    public class OverrideScope : IDisposable
    {
        private readonly ServiceProviderContainer _container;

        internal OverrideScope(ServiceProviderContainer container)
        {
            _container = container;
        }

        public void Dispose()
        {
            _container?.EndOverrideScope(this);
        }
    }
}
