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

            var eValueExpr = entityValue.Body.ReplaceParameter(entityValue.Parameters[0], eParamExpr);

            var unaryExpr = conditionBuilder(eValueExpr);

            var filterClause = Expression.Quote(Expression.Lambda<Func<TEntity, bool>>(unaryExpr, eParamExpr));
            var filterLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(filterClause, rParamExpr);

            return filterLambda.Compile();
        }

        internal static Func<TRequest, Expression<Func<TEntity, bool>>> BuildBinaryFilter<TRequest, TEntity, TRequestProp, TEntityProp>(
            Expression<Func<TRequest, TRequestProp>> requestValue,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<Expression, Expression, Expression> conditionBuilder)
            where TEntity : class
        {
            var riParamExpr = Expression.Parameter(typeof(TRequest), "r");
            var eParamExpr = Expression.Parameter(typeof(TEntity), "e");

            var rValueExpr = requestValue.Body.ReplaceParameter(requestValue.Parameters[0], riParamExpr);
            var eValueExpr = entityValue.Body.ReplaceParameter(entityValue.Parameters[0], eParamExpr);

            var compareExpr = conditionBuilder(rValueExpr, eValueExpr);
            var filterExpr = Expression.Lambda<Func<TRequest, TEntity, bool>>(compareExpr, riParamExpr, eParamExpr);

            var roParamExpr = Expression.Parameter(typeof(TRequest), "ro");
            var body = filterExpr.Body.ReplaceParameter(filterExpr.Parameters[0], roParamExpr);
            
            var filterClause = Expression.Quote(Expression.Lambda<Func<TEntity, bool>>(body, eParamExpr));
            var filterLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(filterClause, roParamExpr);

            return filterLambda.Compile();
        }

        internal static Func<TRequest, Expression<Func<TEntity, bool>>> BuildBinaryFilter<TRequest, TEntity, TValue, TEntityProp>(
            TValue value,
            Expression<Func<TEntity, TEntityProp>> entityValue,
            Func<Expression, Expression, Expression> conditionBuilder)
            where TEntity : class
                => BuildBinaryFilter<TRequest, TEntity, TValue, TEntityProp>(r => value, entityValue, conditionBuilder);
    }
}
