using System;
using System.Linq.Expressions;

namespace UnstableSort.Crudless.Configuration.Builders.Filter
{
    internal static class FilterHelpers
    {
        internal static Func<TRequest, Expression<Func<TEntity, bool>>> BuildUnaryFilter<TRequest, TEntity, TEntityProp>(
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<Expression, Expression> conditionBuilder)
            where TEntity : class
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");

            var eParamExpr = Expression.Parameter(typeof(TEntity), "e");
            var eValueExpr = Expression.Invoke(entityValue, eParamExpr);

            var unaryExpr = conditionBuilder(eValueExpr);

            var filterClause = Expression.Lambda<Func<TEntity, bool>>(unaryExpr, eParamExpr);
            var filterLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(filterClause, rParamExpr);

            return filterLambda.Compile();
        }

        internal static Func<TRequest, Expression<Func<TEntity, bool>>> BuildBinaryFilter<TRequest, TEntity, TRequestProp, TEntityProp>(
            Expression<Func<TRequest, TRequestProp>> requestValue,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<Expression, Expression, Expression> conditionBuilder)
            where TEntity : class
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");
            var eParamExpr = Expression.Parameter(typeof(TEntity), "e");

            var rValueExpr = Expression.Invoke(requestValue, rParamExpr);
            var eValueExpr = Expression.Invoke(entityValue, eParamExpr);

            var compareExpr = conditionBuilder(rValueExpr, eValueExpr);

            var filterClause = Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            var filterLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(filterClause, rParamExpr);

            return filterLambda.Compile();
        }
    }
}
