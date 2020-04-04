using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnstableSort.Crudless.Exceptions;

namespace UnstableSort.Crudless.Configuration.Builders.Select
{
    public class SelectorBuilder<TRequest, TEntity>
        where TEntity : class
    {
        public ISelector Single(Func<TRequest, Expression<Func<TEntity, bool>>> selector)
        {
            return Selector.From(selector);
        }
        
        public ISelector Single(Expression<Func<TRequest, TEntity, bool>> selector)
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");
            var body = selector.Body.ReplaceParameter(selector.Parameters[0], rParamExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(body, selector.Parameters[1]);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public ISelector Single<TRequestKey, TEntityKey>(
            Expression<Func<TRequest, TRequestKey>> requestKeyExpr,
            Expression<Func<TEntity, TEntityKey>> entityKeyExpr)
        {
            var requestKeys = Key.MakeKeys(requestKeyExpr);
            var entityKeys = Key.MakeKeys(entityKeyExpr);

            if (requestKeys == null || requestKeys.Length == 0 ||
                entityKeys == null || entityKeys.Length == 0)
                return null;

            if (requestKeys.Length != entityKeys.Length)
                throw new BadConfigurationException($"Incompatible keys defined for '{typeof(TRequest)}' and '{typeof(TEntity)}'");

            return Single(requestKeys.Zip(entityKeys, (r, e) => ((IKey)r, (IKey)e)));
        }

        public ISelector Single<TRequestKey>(
            Expression<Func<TRequest, TRequestKey>> requestKeyExpr,
            string entityKeyMember)
        {
            var requestKeys = Key.MakeKeys(requestKeyExpr);
            var entityKey = Key.MakeKey<TEntity>(entityKeyMember);

            if (requestKeys == null || requestKeys.Length == 0 || entityKey == null)
                return null;

            if (requestKeys.Length != 1)
                throw new BadConfigurationException($"Incompatible keys defined for '{typeof(TRequest)}' and '{typeof(TEntity)}'");

            return Single(requestKeys[0], entityKey);
        }
        
        public ISelector Single<TEntityKey>(string requestKeyMember,
            Expression<Func<TRequest, TEntityKey>> entityKeyExpr)
        {
            var requestKey = Key.MakeKey<TRequest>(requestKeyMember);
            var entityKeys = Key.MakeKeys(entityKeyExpr);

            if (entityKeys == null || entityKeys.Length == 0 || requestKey == null)
                return null;

            if (entityKeys.Length != 1)
                throw new BadConfigurationException($"Incompatible keys defined for '{typeof(TRequest)}' and '{typeof(TEntity)}'");

            return Single(requestKey, entityKeys[0]);
        }
        
        public ISelector Single(string requestKeyMember, string entityKeyMember)
        {
            var requestKey = Key.MakeKey<TRequest>(requestKeyMember);
            var entityKey = Key.MakeKey<TEntity>(entityKeyMember);
            
            return Single(requestKey, entityKey);
        }

        public ISelector Single(string[] requestKeyMembers, string[] entityKeyMembers)
        {
            var requestKeys = requestKeyMembers.Select(Key.MakeKey<TRequest>).ToArray();
            var entityKeys = entityKeyMembers.Select(Key.MakeKey<TEntity>).ToArray();

            if (requestKeys == null || requestKeys.Length == 0 ||
                entityKeys == null || entityKeys.Length == 0)
                return null;

            if (requestKeys.Length != entityKeys.Length)
                throw new BadConfigurationException($"Incompatible keys defined for '{typeof(TRequest)}' and '{typeof(TEntity)}'");

            return Single(requestKeys.Zip(entityKeys, (r, e) => ((IKey)r, (IKey)e)));
        }

        public ISelector Single(string keyMember)
        {
            var requestKey = Key.MakeKey<TRequest>(keyMember);
            var entityKey = Key.MakeKey<TEntity>(keyMember);

            return Single(requestKey, entityKey);
        }

        public ISelector Single(string[] keyMembers)
        {
            if (keyMembers == null || keyMembers.Length == 0)
                return null;

            var requestKeys = keyMembers.Select(Key.MakeKey<TRequest>).ToArray();
            var entityKeys = keyMembers.Select(Key.MakeKey<TEntity>).ToArray();
            
            return Single(requestKeys.Zip(entityKeys, (r, e) => ((IKey)r, (IKey)e)));
        }

        public ISelector Collection<TKey>(
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
        {
            // TODO: Forward to new builder
            var eParamExpr = Expression.Parameter(typeof(TEntity), "e");
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");

            var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
            var rEnumerableExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);

            var enumerableMethods = typeof(Enumerable).GetMethods();

            var containsInfo = enumerableMethods
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TKey));

            var rContainsExpr = Expression.Call(containsInfo, rEnumerableExpr, eKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public ISelector Collection<TKey>(
            Expression<Func<TRequest, IEnumerable<TKey>>> requestEnumerableExpr,
            string entityKeyProperty)
        {
            // TODO: Forward to new builder
            var eParamExpr = Expression.Parameter(typeof(TEntity), "e");
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");

            var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);
            var rEnumerableExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);

            var enumerableMethods = typeof(Enumerable).GetMethods();

            var containsInfo = enumerableMethods
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TKey));

            var rContainsExpr = Expression.Call(containsInfo, rEnumerableExpr, eKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public ISelector Collection<TIn, TKey>(
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            Expression<Func<TIn, TKey>> requestItemKeyExpr,
            Expression<Func<TEntity, TKey>> entityKeyExpr)
        {
            // TODO: Forward to new builder
            var eParamExpr = Expression.Parameter(typeof(TEntity), "e");
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");

            var eKeyExpr = Expression.Invoke(entityKeyExpr, eParamExpr);
            var reExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);

            var enumerableMethods = typeof(Enumerable).GetMethods();

            var whereInfo = enumerableMethods
                .Single(x => x.Name == "Where" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TIn));

            var rWhereParam = Expression.Parameter(typeof(TIn));
            var compareExpr = Expression.NotEqual(rWhereParam, Expression.Constant(default(TIn), typeof(TIn)));
            var whereLambda = Expression.Lambda(compareExpr, rWhereParam);

            var selectInfo = enumerableMethods
                .Single(x => x.Name == "Select" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TIn), typeof(TKey));

            var containsInfo = enumerableMethods
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TKey));

            var rWhereExpr = Expression.Call(whereInfo, reExpr, whereLambda);
            var rReduceExpr = Expression.Call(selectInfo, rWhereExpr, requestItemKeyExpr);
            var rContainsExpr = Expression.Call(containsInfo, rReduceExpr, eKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        public ISelector Collection<TIn>(
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            string requestItemKeyProperty,
            string entityKeyProperty)
        {
            // TODO: Forward to new builder
            var eParamExpr = Expression.Parameter(typeof(TEntity), "e");
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");

            var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);
            var reExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);

            var enumerableMethods = typeof(Enumerable).GetMethods();

            var whereInfo = enumerableMethods
                .Single(x => x.Name == "Where" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TIn));

            var rWhereParam = Expression.Parameter(typeof(TIn));
            var compareExpr = Expression.NotEqual(rWhereParam, Expression.Constant(default(TIn), typeof(TIn)));
            var whereLambda = Expression.Lambda(compareExpr, rWhereParam);

            var selectInfo = enumerableMethods
                .Single(x => x.Name == "Select" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TIn), eKeyExpr.Type);

            var containsInfo = enumerableMethods
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(eKeyExpr.Type);

            var iParamExpr = Expression.Parameter(typeof(TIn));
            var iKeyExpr = Expression.PropertyOrField(iParamExpr, requestItemKeyProperty);
            var iExpr = Expression.Lambda(iKeyExpr, iParamExpr);

            var rWhereExpr = Expression.Call(whereInfo, reExpr, whereLambda);
            var rReduceExpr = Expression.Call(selectInfo, rWhereExpr, iExpr);
            var rContainsExpr = Expression.Call(containsInfo, rReduceExpr, eKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        internal ISelector Single(IKey requestKey, IKey entityKey)
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");
            var eParamExpr = Expression.Parameter(typeof(TEntity), "e");

            var rKeyExpr = Expression.Invoke(requestKey.KeyExpression, rParamExpr);
            var eKeyExpr = Expression.Invoke(entityKey.KeyExpression, eParamExpr);
            var compareExpr = Expression.Equal(eKeyExpr, rKeyExpr);

            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(compareExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        internal ISelector Single(IEnumerable<(IKey, IKey)> keys)
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");
            var eParamExpr = Expression.Parameter(typeof(TEntity), "e");

            var compareExprs = keys
                .Select(pair => Expression.Equal(
                    Expression.Invoke(pair.Item1.KeyExpression, rParamExpr),
                    Expression.Invoke(pair.Item2.KeyExpression, eParamExpr)));

            var accumExpr = compareExprs.Aggregate((left, right) => Expression.AndAlso(left, right));
            
            var selectorClause = Expression.Lambda<Func<TEntity, bool>>(accumExpr, eParamExpr);
            var selectorLambda = Expression.Lambda<Func<TRequest, Expression<Func<TEntity, bool>>>>(selectorClause, rParamExpr);

            return Selector.From(selectorLambda.Compile());
        }

        internal ISelector Collection<TIn>(
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            IKey entityKey,
            IKey itemKey)
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest), "r");
            var enumerableMethods = typeof(Enumerable).GetMethods();

            var whereInfo = enumerableMethods
                .Single(x => x.Name == "Where" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TIn));

            var selectInfo = enumerableMethods
                .Single(x => x.Name == "Select" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(typeof(TIn), itemKey.KeyType);

            var toArrayInfo = enumerableMethods
                .Single(x => x.Name == nameof(Enumerable.ToArray))
                .MakeGenericMethod(itemKey.KeyType);

            var reExpr = Expression.Invoke(requestEnumerableExpr, rParamExpr);
            var rWhereParam = Expression.Parameter(typeof(TIn));
            var compareExpr = Expression.NotEqual(rWhereParam, Expression.Constant(default(TIn), typeof(TIn)));
            var whereLambda = Expression.Lambda(compareExpr, rWhereParam);

            var rWhereExpr = Expression.Call(whereInfo, reExpr, whereLambda);
            var rReduceExpr = Expression.Call(selectInfo, rWhereExpr, itemKey.KeyExpression);
            var arrExpr = Expression.Call(toArrayInfo, rReduceExpr);

            var makeSelectorInfo = GetType()
                .GetMethod(nameof(MakeContainsSelector), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(itemKey.KeyType);

            var selector = (Func<TRequest, Expression<Func<TEntity, bool>>>)
                makeSelectorInfo.Invoke(null, new object[] { Expression.Lambda(arrExpr, rParamExpr), entityKey });

            return Selector.From(selector);
        }

        private static Func<TRequest, Expression<Func<TEntity, bool>>> MakeContainsSelector<TKey>(
            LambdaExpression makeIdExpr, 
            IKey entityKey)
        {
            var containsInfo = typeof(Enumerable)
                .GetMethods()
                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TKey));

            if (makeIdExpr is Expression<Func<TRequest, TKey[]>> makeIdTypedExpr)
            {
                var makeIdFunc = makeIdTypedExpr.Compile();

                var eParamExpr = Expression.Parameter(typeof(TEntity), "e");
                var eKeyExpr = Expression.Invoke(entityKey.KeyExpression, eParamExpr);

                return request =>
                {
                    var itemIds = makeIdFunc(request);
                    var itemIdExpr = Expression.Constant(itemIds, typeof(TKey[]));
                    var rContainsExpr = Expression.Call(containsInfo, itemIdExpr, eKeyExpr);

                    return Expression.Lambda<Func<TEntity, bool>>(rContainsExpr, eParamExpr);
                };
            }

            throw new InvalidOperationException();
        }
    }
}
