using SimpleInjector;
using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.Integration.SimpleInjector
{
    public static class ContextExtensions
    {
        public static ConditionalContext ToConditionalContext(this PredicateContext context)
            => new ConditionalContext(context.ServiceType, context.ImplementationType, context.Handled);

        public static DecoratorConditionalContext ToConditionalContext(this DecoratorPredicateContext context)
            => new DecoratorConditionalContext(context.ServiceType, context.ImplementationType, context.AppliedDecorators);
    }
}
