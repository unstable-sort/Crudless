using System;
using System.Collections.ObjectModel;

namespace UnstableSort.Crudless.Common.ServiceProvider
{
    public class ConditionalContext
    {
        public ConditionalContext(Type serviceType, Type implementationType, bool handled)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Handled = handled;
        }

        public Type ServiceType { get; }

        public Type ImplementationType { get; }

        public bool Handled { get; }
    }

    public class DecoratorConditionalContext
    {
        public DecoratorConditionalContext(Type serviceType,
            Type implementationType,
            ReadOnlyCollection<Type> appliedDecorators)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            AppliedDecorators = appliedDecorators;
        }

        public Type ServiceType { get; }

        public Type ImplementationType { get; }

        public ReadOnlyCollection<Type> AppliedDecorators { get; }
    }
}
