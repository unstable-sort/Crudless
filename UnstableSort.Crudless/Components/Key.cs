using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UnstableSort.Crudless
{
    public interface IKey
    {
        Type KeyType { get; }

        LambdaExpression KeyExpression { get; }
    }

    public class Key : IKey
    {
        public Type KeyType { get; }

        public LambdaExpression KeyExpression { get; }

        public Key(Type keyType, LambdaExpression keyExpr)
        {
            KeyType = keyType;
            KeyExpression = keyExpr;
        }

        public static Key Identity<TSource>()
        {
            var sParamExpr = Expression.Parameter(typeof(TSource), "src");
            var keyExpr = Expression.Lambda<Func<TSource, TSource>>(sParamExpr, sParamExpr);

            return new Key(typeof(TSource), keyExpr);
        }

        public static Key MakeKey<TSource>(string keyMember)
        {
            if (string.IsNullOrWhiteSpace(keyMember))
                return Key.Identity<TSource>();

            var sParamExpr = Expression.Parameter(typeof(TSource), "src");
            var sKeyExpr = Expression.PropertyOrField(sParamExpr, keyMember);
            Key key;

            switch (sKeyExpr.Member)
            {
                case FieldInfo f:
                    key = new Key(f.FieldType, Expression.Lambda(Expression.Field(sParamExpr, f.Name), sParamExpr));
                    break;

                case PropertyInfo p:
                    key = new Key(p.PropertyType, Expression.Lambda(Expression.Property(sParamExpr, p.Name), sParamExpr));
                    break;

                default:
                    throw new ArgumentException("Key expressions may only reference properties and/or fields.");
            }

            return key;
        }

        public static Key[] MakeKeys<TSource, TKey>(Expression<Func<TSource, TKey>> keyExpr)
        {
            if (keyExpr.Body is ParameterExpression parameterExpression)
                return new[] { new Key(typeof(TSource), keyExpr) };

            if (keyExpr.Body is NewExpression newExpression)
            {
                var sourceExpr = Expression.Parameter(typeof(TSource), "src");

                return newExpression.Members
                    .Select(x =>
                    {
                        switch (x)
                        {
                            case FieldInfo f:
                                return new Key(f.FieldType, Expression.Lambda(Expression.Field(sourceExpr, f.Name), sourceExpr));

                            case PropertyInfo p:
                                return new Key(p.PropertyType, Expression.Lambda(Expression.Property(sourceExpr, p.Name), sourceExpr));

                            default:
                                throw new ArgumentException("Key expressions may only reference properties and/or fields.");
                        }
                    })
                    .ToArray();
            }
            else
            {
                var memberExpression = (keyExpr.Body is UnaryExpression unaryExpression)
                    ? unaryExpression.Operand as MemberExpression
                    : keyExpr.Body as MemberExpression;

                if (memberExpression == null)
                    throw new ArgumentException($"Invalid key expression: '{keyExpr}'", nameof(keyExpr));

                return new[] { new Key(typeof(TKey), keyExpr) };
            }
        }
    }
}
