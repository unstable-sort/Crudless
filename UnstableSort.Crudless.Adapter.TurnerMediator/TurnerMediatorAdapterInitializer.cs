using System.Reflection;
using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.Adapter.TurnerMediator
{
    public class CrudlessToTurnerMediatorAdapterInitializer : CrudlessInitializationTask
    {
        private static bool IfNotHandled(ConditionalContext c) => !c.Handled;

        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            var requestAssemblies = new[] { typeof(CrudlessRequest<>).Assembly };

            container.Register(typeof(CrudlessRequest<>), requestAssemblies);
            container.Register(typeof(CrudlessRequest<,>), requestAssemblies);

            container.RegisterConditional(typeof(Turner.Infrastructure.Mediator.IRequestHandler<>), typeof(CrudlessRequestHandler<>), IfNotHandled);
            container.RegisterConditional(typeof(Turner.Infrastructure.Mediator.IRequestHandler<,>), typeof(CrudlessRequestHandler<,>), IfNotHandled);
        }
    }

    public class TurnerToCrudlessMediatorAdapterInitializer : CrudlessInitializationTask
    {
        private static bool IfNotHandled(ConditionalContext c) => !c.Handled;

        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            var requestAssemblies = new[] { typeof(TurnerRequest<>).Assembly };

            container.Register(typeof(TurnerRequest<>), requestAssemblies);
            container.Register(typeof(TurnerRequest<,>), requestAssemblies);

            container.RegisterConditional(typeof(Mediator.IRequestHandler<>), typeof(TurnerRequestHandler<>), IfNotHandled);
            container.RegisterConditional(typeof(Mediator.IRequestHandler<,>), typeof(TurnerRequestHandler<,>), IfNotHandled);
        }
    }
}
