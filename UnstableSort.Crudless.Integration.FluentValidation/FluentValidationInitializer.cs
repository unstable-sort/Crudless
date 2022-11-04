using System.Reflection;
using FluentValidation;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Integration.FluentValidation
{
    public class FluentValidationInitializer : CrudlessInitializationTask
    {
        private static bool IfNotHandled(ConditionalContext c) => !c.Handled;

        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            container.Register(typeof(IValidator<>), assemblies);

            container.RegisterConditional(typeof(IRequestValidator<>), typeof(FluentRequestValidator<>), IfNotHandled);
        }

        public override bool Supports(string option)
        {
            if (option == "FluentValidation")
                return true;
            
            return base.Supports(option);
        }
    }
}
