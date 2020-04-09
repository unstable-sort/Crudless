using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UnstableSort.Crudless.Context
{
    public static class QueryableMethods
    {
        public static MethodInfo Contains { get; }

        public static MethodInfo CountWithoutPredicate { get; }

        public static MethodInfo CountWithPredicate { get; }

        public static MethodInfo LongCountWithoutPredicate { get; }

        public static MethodInfo LongCountWithPredicate { get; }
        
        public static MethodInfo FirstWithoutPredicate { get; }

        public static MethodInfo FirstWithPredicate { get; }

        public static MethodInfo FirstOrDefaultWithoutPredicate { get; }

        public static MethodInfo FirstOrDefaultWithPredicate { get; }

        public static MethodInfo SingleWithoutPredicate { get; }

        public static MethodInfo SingleWithPredicate { get; }

        public static MethodInfo SingleOrDefaultWithoutPredicate { get; }

        public static MethodInfo SingleOrDefaultWithPredicate { get; }

        static QueryableMethods()
        {
            var queryableMethods = typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();

            Contains = queryableMethods.Single(x => 
                x.Name == nameof(Queryable.Contains) && 
                x.GetParameters().Length == 2);

            CountWithoutPredicate = queryableMethods.Single(x => 
                x.Name == nameof(Queryable.Count) && 
                x.GetParameters().Length == 1);

            CountWithPredicate = queryableMethods.Single(x => 
                x.Name == nameof(Queryable.Count) && 
                x.GetParameters().Length == 2 && 
                IsExpressionOfFunc(x.GetParameters()[1].ParameterType));

            LongCountWithoutPredicate = queryableMethods.Single(x => 
                x.Name == nameof(Queryable.LongCount) && 
                x.GetParameters().Length == 1);

            LongCountWithPredicate = queryableMethods.Single(x => 
                x.Name == nameof(Queryable.LongCount) && 
                x.GetParameters().Length == 2 && 
                IsExpressionOfFunc(x.GetParameters()[1].ParameterType));

            FirstWithoutPredicate = queryableMethods.Single(x => 
                x.Name == nameof(Queryable.First) && 
                x.GetParameters().Length == 1);

            FirstWithPredicate = queryableMethods.Single(x => 
                x.Name == nameof(Queryable.First) && 
                x.GetParameters().Length == 2 && 
                IsExpressionOfFunc(x.GetParameters()[1].ParameterType));

            FirstOrDefaultWithoutPredicate = queryableMethods.Single(x => 
                x.Name == nameof(Queryable.FirstOrDefault) && 
                x.GetParameters().Length == 1);

            FirstOrDefaultWithPredicate = queryableMethods.Single(x => 
                x.Name == nameof(Queryable.FirstOrDefault) && 
                x.GetParameters().Length == 2 && 
                IsExpressionOfFunc(x.GetParameters()[1].ParameterType));

            SingleWithoutPredicate = queryableMethods.Single(x => 
                x.Name == nameof(Queryable.Single) && 
                x.GetParameters().Length == 1);

            SingleWithPredicate = queryableMethods.Single(x => 
                x.Name == nameof(Queryable.Single) && 
                x.GetParameters().Length == 2 && 
                IsExpressionOfFunc(x.GetParameters()[1].ParameterType));

            SingleOrDefaultWithoutPredicate = queryableMethods.Single(x => 
                x.Name == nameof(Queryable.SingleOrDefault) && 
                x.GetParameters().Length == 1);

            SingleOrDefaultWithPredicate = queryableMethods.Single(x => 
                x.Name == nameof(Queryable.SingleOrDefault) && 
                x.GetParameters().Length == 2 && 
                IsExpressionOfFunc(x.GetParameters()[1].ParameterType));

            static bool IsExpressionOfFunc(Type type, int funcGenericArgs = 2)
                => type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(Expression<>) &&
                    type.GetGenericArguments()[0].IsGenericType &&
                    type.GetGenericArguments()[0].GetGenericArguments().Length == funcGenericArgs;
        }
    }
}
