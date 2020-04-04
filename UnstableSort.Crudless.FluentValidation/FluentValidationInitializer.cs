using System.Reflection;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.FluentValidation
{
    public class FluentValidationInitializer : CrudlessInitializationTask
    {
        private static bool IfNotHandled(ConditionalContext c) => !c.Handled;

        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            container.RegisterConditional(typeof(IRequestValidator<>), typeof(FluentRequestValidator<>), c => !c.Handled);
        }

        public override bool Supports(string option)
        {
            if (option == "FluentValidation")
                return true;
            
            return base.Supports(option);
        }
    }
}
