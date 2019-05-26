using System.Reflection;
using SimpleInjector;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.FluentValidation
{
    public class FluentValidationInitializer : ICrudlessInitializationTask
    {
        private static bool IfNotHandled(PredicateContext c) => !c.Handled;

        public void Run(Container container, Assembly[] assemblies, CrudlessOptions options)
        {
            container.RegisterConditional(typeof(IRequestValidator<>), typeof(FluentRequestValidator<>), c => !c.Handled);
        }
    }
}
