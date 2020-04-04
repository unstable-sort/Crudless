using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using UnstableSort.Crudless.Exceptions;

namespace UnstableSort.Crudless.Context
{
    internal static class QueryableExtensions
    {
        private static readonly Dictionary<string, MethodInfo> _methods =
            new Dictionary<string, MethodInfo>
            {
                { "FirstOrDefault", GetMethod(nameof(Queryable.FirstOrDefault)) },
                { "FirstOrDefaultPredicate", GetMethod(nameof(Queryable.FirstOrDefault), 1) },
                { "SingleOrDefault", GetMethod(nameof(Queryable.SingleOrDefault))},
                { "SingleOrDefaultPredicate", GetMethod(nameof(Queryable.SingleOrDefault), 1) },
                { "Count", GetMethod(nameof(Queryable.Count)) },
                { "CountPredicate", GetMethod(nameof(Queryable.Count), 1) }
            };
        
        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.FirstOrDefaultWithoutPredicate, source, token);

        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.FirstOrDefaultWithPredicate, source, predicate, token);

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, 
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.SingleOrDefaultWithoutPredicate, source, token);

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.SingleOrDefaultWithPredicate, source, predicate, token);

        public static Task<TResult> ProjectSingleOrDefaultAsync<TSource, TResult>(this IQueryable<TSource> source,
            IConfigurationProvider mapperConfigProvider,
            CancellationToken token = default(CancellationToken))
            => source.ProjectTo<TResult>(mapperConfigProvider).SingleOrDefaultAsync(token);

        public static Task<TResult> ProjectSingleOrDefaultAsync<TSource, TResult>(this IQueryable<TSource> source,
            IConfigurationProvider mapperConfigProvider,
            Expression<Func<TResult, bool>> predicate,
            CancellationToken token = default(CancellationToken))
            => source.ProjectTo<TResult>(mapperConfigProvider).SingleOrDefaultAsync(predicate, token);

        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, Task<int>>(QueryableMethods.CountWithoutPredicate, source, token);

        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> predicate,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, Task<int>>(QueryableMethods.CountWithPredicate, source, predicate, token);

        public static async Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
        {
            var list = new List<TSource>();

            await foreach (var element in source.AsAsyncEnumerable().WithCancellation(token))
                list.Add(element);

            return list;
        }

        public static Task<List<TResult>> ProjectToListAsync<TSource, TResult>(this IQueryable<TSource> source,
            IConfigurationProvider mapperConfigProvider,
            CancellationToken token = default(CancellationToken))
            => source.ProjectTo<TResult>(mapperConfigProvider).ToListAsync(token);

        public static async Task<TSource[]> ToArrayAsync<TSource>(this IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
            => (await source.ToListAsync(token)).ToArray();

        public static Task<TResult[]> ProjectToArrayAsync<TSource, TResult>(this IQueryable<TSource> source,
            IConfigurationProvider mapperConfigProvider,
            CancellationToken token = default(CancellationToken))
            => source.ProjectTo<TResult>(mapperConfigProvider).ToArrayAsync(token);

        private static TResult ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, TResult>(operatorMethodInfo, source, (Expression)null, token);

        private static TResult ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            LambdaExpression expression,
            CancellationToken token = default(CancellationToken))
            => ExecuteAsync<TSource, TResult>(operatorMethodInfo, source, Expression.Quote(expression), token);

        private static TResult ExecuteAsync<TSource, TResult>(
            MethodInfo operatorMethodInfo,
            IQueryable<TSource> source,
            Expression expression,
            CancellationToken token = default(CancellationToken))
        {
            if (operatorMethodInfo.IsGenericMethod)
            {
                operatorMethodInfo = operatorMethodInfo.GetGenericArguments().Length == 2
                    ? operatorMethodInfo.MakeGenericMethod(typeof(TSource), typeof(TResult).GetGenericArguments().Single())
                    : operatorMethodInfo.MakeGenericMethod(typeof(TSource));
            }

            var executeAsyncMethod = source.Provider.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .SingleOrDefault(x =>
                    x.Name == "ExecuteAsync" &&
                    x.IsGenericMethod &&
                    x.GetParameters().Length == 2);

            if (executeAsyncMethod == null)
                throw new InvalidQueryProviderTypeException();

            var execute = executeAsyncMethod.MakeGenericMethod(typeof(TResult));

            try
            {
                return (TResult)execute
                    .Invoke(source.Provider, new object[]
                    {
                    Expression.Call(
                        instance: null,
                        method: operatorMethodInfo,
                        arguments: expression == null
                            ? new[] { source.Expression }
                            : new[] { source.Expression, expression }),
                    token
                    });
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;
                
                throw e;
            }
        }

        private static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this IQueryable<TSource> source)
        {
            if (source is IAsyncEnumerable<TSource> asyncEnumerable)
                return asyncEnumerable;

            throw new ArgumentException($"'{nameof(source)}' is not async.");
        }

        private static MethodInfo GetMethod<TResult>(string name,
            int parameterCount = 0,
            Func<MethodInfo, bool> predicate = null)
        {
            return GetMethod(name, parameterCount, 
                x => x.ReturnType == typeof(TResult) && (predicate == null || predicate(x)));
        }

        private static MethodInfo GetMethod(string name,
            int parameterCount = 0,
            Func<MethodInfo, bool> predicate = null)
        {
            return typeof(Queryable)
                .GetTypeInfo()
                .GetDeclaredMethods(name)
                .Single(x => x.GetParameters().Length == parameterCount + 1
                          && (predicate == null || predicate(x)));
        }
    }
}
