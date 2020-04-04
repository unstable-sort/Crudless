using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace UnstableSort.Crudless.Context.Utilities
{
    public interface IAsyncQueryProvider : IQueryProvider
    {
        TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default);
    }
    
    public static class AsyncQueryProviderExtensions
    {
        public static IAsyncQueryProvider AsAsyncQueryProvider(this IQueryProvider queryProvider)
        {
            if (queryProvider is null)
                throw new ArgumentNullException(nameof(queryProvider));

            if (queryProvider is IAsyncQueryProvider asyncQueryProvider)
                return asyncQueryProvider;

            return new AsyncAdaptedQueryProvider(queryProvider);
        }

        public class AsyncAdaptedQueryProvider : IAsyncQueryProvider
        {
            private static readonly MethodInfo _createQuery
                = typeof(AsyncAdaptedQueryProvider)
                    .GetRuntimeMethods()
                    .Single(x => x.Name == nameof(CreateQuery) && x.IsGenericMethod);
            
            private readonly IQueryProvider _queryProvider;

            public AsyncAdaptedQueryProvider(IQueryProvider queryProvider)
            {
                _queryProvider = queryProvider;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return (IQueryable)_createQuery
                    .MakeGenericMethod(expression.Type.GetSequenceType())
                    .Invoke(this, new object[] { expression });
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
                => new AsyncQueryable<TElement>(_queryProvider, expression);

            public object Execute(Expression expression)
                => _queryProvider.Execute(expression);

            public TResult Execute<TResult>(Expression expression)
                => _queryProvider.Execute<TResult>(expression);

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken token)
            {
                if (typeof(Task).IsAssignableFrom(typeof(TResult)))
                {
                    try
                    {
                        if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(Task<>))
                        {
                            var executeMethod = typeof(AsyncAdaptedQueryProvider)
                                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                .Single(x => x.Name == nameof(ExecuteSync) && x.IsGenericMethod)
                                .MakeGenericMethod(typeof(TResult).GetGenericArguments()[0]);

                            return (TResult)executeMethod.Invoke(this, new object[] { expression });
                        }
                        else
                        {
                            var executeMethod = typeof(AsyncAdaptedQueryProvider)
                                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                .Single(x => x.Name == nameof(ExecuteSync) && !x.IsGenericMethod);

                            return (TResult)executeMethod.Invoke(this, new object[] { expression });
                        }
                    } 
                    catch (TargetInvocationException e)
                    {
                        if (e.InnerException != null)
                            throw e.InnerException;
                        
                        throw e;
                    }
                }
                else if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
                {
                    var executeMethod = typeof(AsyncAdaptedQueryProvider)
                                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                .Single(x => x.Name == nameof(ExecuteStream) && x.IsGenericMethod)
                                .MakeGenericMethod(typeof(TResult).GetGenericArguments()[0]);

                    return (TResult)executeMethod.Invoke(this, new object[] { expression });
                }

                return _queryProvider.Execute<TResult>(expression);
            }

            private async IAsyncEnumerable<TResult> ExecuteStream<TResult>(Expression expression)
            {
                var results = Task.FromResult(Execute<IEnumerable<TResult>>(expression));

                foreach (var result in await results)
                    yield return result;
            }

            private Task ExecuteSync(Expression expression)
                => Task.FromResult(Execute(expression));

            private Task<TResult> ExecuteSync<TResult>(Expression expression)
                => Task.FromResult(Execute<TResult>(expression));
        }
    }
}
