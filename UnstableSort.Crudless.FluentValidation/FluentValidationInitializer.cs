using System.Reflection;
using SimpleInjector;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.FluentValidation
{
    public class FluentValidationInitializer : CrudlessInitializationTask
    {
        private static bool IfNotHandled(PredicateContext c) => !c.Handled;

        public override void Run(Container container, Assembly[] assemblies, CrudlessOptions options)
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
