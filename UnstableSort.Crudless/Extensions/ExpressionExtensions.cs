using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace UnstableSort.Crudless
{
    internal static class ExpressionExtensions
    {
        internal static Expression ReplaceParameter(this Expression expression, ParameterExpression source, Expression target)
        {
            var replacer = new ParameterReplacer(source, target);

            return replacer.Visit(expression);
        }

        internal static Expression SubstituteParameter(this Expression expression, Type source, Type target)
        {
            var replacer = new ParameterSubtituter(source, target);

            return replacer.Visit(expression);
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            public ParameterExpression Source { get; }

            public Expression Target { get; }

            public ParameterReplacer(ParameterExpression source, Expression target)
            {
                Source = source;
                Target = target;
            }

            protected override Expression VisitParameter(ParameterExpression node)
                => node == Source ? Target : base.VisitParameter(node);
        }

        private class ParameterSubtituter : ExpressionVisitor
        {
            public ReadOnlyCollection<ParameterExpression> Parameters;

            public Type Source { get; }

            public Type Target { get; }

            public ParameterSubtituter(Type source, Type target)
            {
                Source = source;
                Target = target;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return Parameters?.FirstOrDefault(x => x.Name == node.Name)
                    ?? (node.Type == Source
                        ? Expression.Parameter(Target, node.Name)
                        : node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                Parameters = VisitAndConvert(node.Parameters, nameof(VisitLambda));

                return Expression.Lambda(Visit(node.Body), Parameters);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Member.DeclaringType == Source)
                    return Expression.PropertyOrField(Visit(node.Expression), node.Member.Name);

                return base.VisitMember(node);
            }
        }
    }
}
