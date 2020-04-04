using System.Reflection;
using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.Adapter.TurnerMediator
{
    public class TurnerMediatorAdapterInitializer : CrudlessInitializationTask
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
}
