using SimpleInjector;
using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.ServiceProvider.SimpleInjector
{
    public static class ContainerExtensions
    {
        public static ServiceProviderContainer AsServiceProvider(this Container container)
            => new SimpleInjectorServiceProviderContainer(container);
    }
}
