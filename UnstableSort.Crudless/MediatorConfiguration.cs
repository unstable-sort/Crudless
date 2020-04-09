using System.Reflection;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless
{
    internal abstract class MediatorInitializer : CrudlessInitializationTask
    {
        protected MediatorInitializer(bool scopeRequests)
        {
            ScopeRequests = scopeRequests;
        }

        public bool ScopeRequests { get; }
    }

    internal class DynamicMediatorInitializer : MediatorInitializer
    {
        public DynamicMediatorInitializer(bool scopeRequests = true) : base(scopeRequests)
        {
        }

        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            container.Register<IMediator>(() => new DynamicDispatchMediator(container, ScopeRequests));
        }
    }

    internal class DefaultMediatorInitializer : DynamicMediatorInitializer
    {
        public DefaultMediatorInitializer(bool scopeRequests = true) : base(scopeRequests)
        {
        }
    }
    
    public static class IncludeMediatorInitializer
    {
        public static CrudlessInitializer UseDynamicMediator(this CrudlessInitializer initializer, bool scopeRequests = true)
        {
            return initializer
                .RemoveInitializers<MediatorInitializer>()
                .AddInitializer(new DynamicMediatorInitializer(scopeRequests));
        }
    }
}
