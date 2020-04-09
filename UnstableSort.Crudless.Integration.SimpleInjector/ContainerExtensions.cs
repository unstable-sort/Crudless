using SimpleInjector;
using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.Integration.SimpleInjector
{
    public static class ContainerExtensions
    {
        public static ServiceProviderContainer AsServiceProvider(this Container container)
            => new SimpleInjectorServiceProviderContainer(container);
    }
}
