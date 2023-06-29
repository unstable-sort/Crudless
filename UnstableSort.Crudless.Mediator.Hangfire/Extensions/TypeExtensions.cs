using System;

namespace UnstableSort.Crudless.Mediator.Hangfire.Extensions
{
    internal static class TypeExtensions
    {
        public static bool IsSubclassOfGenericType(this Type derivedType, Type genericType)
        {
            while (derivedType != null && derivedType != typeof(object))
            {
                var currentType = derivedType.IsGenericType
                    ? derivedType.GetGenericTypeDefinition() 
                    : derivedType;

                if (genericType == currentType)
                    return true;

                derivedType = derivedType.BaseType;
            }

            return false;
        }
    }
}
