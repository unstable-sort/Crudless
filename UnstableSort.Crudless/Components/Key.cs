using System;
using System.Linq.Expressions;

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
    }
}
