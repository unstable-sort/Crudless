using System.Reflection;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless
{
    internal abstract class MediatorInitializer : CrudlessInitializationTask
    {
    }

    internal class DynamicMediatorInitializer : MediatorInitializer
    {
        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            container.Register<IMediator>(() => new DynamicDispatchMediator(container));
        }
    }

    internal class DefaultMediatorInitializer : DynamicMediatorInitializer
    {
    }
    
    public static class IncludeMediatorInitializer
    {
        public static CrudlessInitializer UseDynamicMediator(this CrudlessInitializer initializer)
        {
            return initializer
                .RemoveInitializers<MediatorInitializer>()
                .AddInitializer(new DynamicMediatorInitializer());
        }
    }
}
