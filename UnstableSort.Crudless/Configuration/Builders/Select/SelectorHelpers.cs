using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UnstableSort.Crudless.Configuration.Builders.Select
{
    internal static class SelectorHelpers
    {
        public static ISelector BuildSingle<TRequest, TEntity>(IKey kRequest, IKey kEntity)
            where TEntity : class
        {
            var eParamExpr = Expression.Parameter(typeof(TEntity), "e");
            var riParamExpr = Expression.Parameter(typeof(TRequest), "ri");

            var rKeyExpr = kRequest.KeyExpression.Body.ReplaceParameter(kRequest.KeyExpression.Parameters[0], riParamExpr);
            var eKeyExpr = kEntity.KeyExpression.Body.ReplaceParameter(kEntity.KeyExpression.Parameters[0], eParamExpr);

            var compareExpr = Expression.Equal(eKeyExpr, rKeyExpr);
            var keyExpr = Expression.Lambda<Func<TRequest, TEntity, bool>>(compareExpr, riParamExpr, eParamExpr);

            var roParamExpr = Expression.Parameter(typeof(TRequest), "ro");
            var body = keyExpr.Body.ReplaceParameter(keyExpr.Parameters[0], roParamExpr);

            var selectorClause = Expression.Quote(Expression.Lambda<Func<TEntity, bool>>(body, keyExpr.Parameters[1]));
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, roParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public static ISelector BuildSingle<TRequest, TEntity>(IEnumerable<(IKey, IKey)> kBoth)
            where TEntity : class
        {
            var keys = kBoth.ToArray();
            if (keys.Length == 1)
                return BuildSingle<TRequest, TEntity>(keys[0].Item1, keys[0].Item2);

            var eParamExpr = Expression.Parameter(typeof(TEntity), "e");
            var riParamExpr = Expression.Parameter(typeof(TRequest), "ri");

            var compareExprs = kBoth
                .Select(pair =>
                {
                    var rKeyExpr = pair.Item1.KeyExpression.Body.ReplaceParameter(pair.Item1.KeyExpression.Parameters[0], riParamExpr);
                    var eKeyExpr = pair.Item2.KeyExpression.Body.ReplaceParameter(pair.Item2.KeyExpression.Parameters[0], eParamExpr);

                    return Expression.Equal(eKeyExpr, rKeyExpr);
                });

            var accumExpr = compareExprs.Aggregate((left, right) => Expression.AndAlso(left, right));
            var keyExpr = Expression.Lambda<Func<TRequest, TEntity, bool>>(accumExpr, riParamExpr, eParamExpr);

            var roParamExpr = Expression.Parameter(typeof(TRequest), "ro");
            var body = keyExpr.Body.ReplaceParameter(keyExpr.Parameters[0], roParamExpr);

            var selectorClause = Expression.Quote(Expression.Lambda<Func<TEntity, bool>>(body, keyExpr.Parameters[1]));
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, roParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public static ISelector BuildCollection<TRequest, TItem, TEntity>(
            Expression<Func<TRequest, ICollection<TItem>>> requestItems,
            IKey kEntity,
            IKey kItem)
            where TEntity : class
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");
            var enumerableMethods = typeof(Enumerable).GetMethods();

            var whereInfo = enumerableMethods
                .Single(x => x.Name == nameof(Enumerable.Where) &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TItem));

            var selectInfo = enumerableMethods
                .Single(x => x.Name == nameof(Enumerable.Select) &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TItem), kItem.KeyType);

            var toArrayInfo = enumerableMethods
                .Single(x => x.Name == nameof(Enumerable.ToArray))
                .MakeGenericMethod(kItem.KeyType);

            var reExpr = Expression.Invoke(requestItems, rParamExpr);
            var rWhereParam = Expression.Parameter(typeof(TItem));
            var compareExpr = Expression.NotEqual(rWhereParam, Expression.Constant(default(TItem), typeof(TItem)));
            var whereLambda = Expression.Lambda(compareExpr, rWhereParam);

            var rWhereExpr = Expression.Call(whereInfo, reExpr, whereLambda);
            var rReduceExpr = Expression.Call(selectInfo, rWhereExpr, kItem.KeyExpression);
            var arrExpr = Expression.Call(toArrayInfo, rReduceExpr);

            var makeSelectorInfo = typeof(SelectorHelpers)
                .GetMethod(nameof(BuildContainsSelector), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(typeof(TRequest), typeof(TEntity), kItem.KeyType);

            try
            {
                var selector = (Func<TRequest, Expression<Func<TEntity, bool>>>)
                    makeSelectorInfo.Invoke(null, new object[] { Expression.Lambda(arrExpr, rParamExpr), kEntity });

                return Selector.From(selector);
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw e;
            }
        }

        private static Func<TRequest, Expression<Func<TEntity, bool>>> BuildContainsSelector<TRequest, TEntity, TKey>(
            LambdaExpression getItemId,
            IKey kEntity)
            where TEntity : class
        {
            var containsInfo = typeof(Enumerable)
                .GetMethods()
                .Single(x => x.Name == nameof(Enumerable.Contains) && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TKey));

            if (getItemId is Expression<Func<TRequest, TKey[]>> getItemIdExpr)
            {
                var makeIdFunc = getItemIdExpr.Compile();

                var eParamExpr = Expression.Parameter(typeof(TEntity), "e");
                
                return request =>
                {
                    var itemIds = makeIdFunc(request);
                    var itemIdExpr = Expression.Constant(itemIds, typeof(TKey[]));

                    var eKeyExpr = kEntity.KeyExpression.Body.ReplaceParameter(kEntity.KeyExpression.Parameters[0], eParamExpr);
                    var rContainsExpr = Expression.Call(containsInfo, itemIdExpr, eKeyExpr);

                    return Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);
                };
            }

            throw new InvalidOperationException();
        }
    }
}
