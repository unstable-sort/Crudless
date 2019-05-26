using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace UnstableSort.Crudless.Mediator
{
    public class DynamicDispatchMediator : IMediator
    {
        private static ConcurrentDictionary<Type, HandlerCacheItem> _handlerCache
            = new ConcurrentDictionary<Type, HandlerCacheItem>();

        private readonly Func<Type, object> _resolver;

        public DynamicDispatchMediator(Func<Type, object> resolver)
        {
            _resolver = resolver;
        }
        
        public Task<Response<TResult>> HandleAsync<TResult>(IRequest<TResult> request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var requestType = request.GetType();

            if (_handlerCache.TryGetValue(requestType, out var cachedItem))
                return (Task<Response<TResult>>) cachedItem.HandlerMethod.Invoke(this, new object[] { request, cachedItem.HandlerType, token });
            
            if (typeof(TResult) == typeof(NoResult) && TryNakedHandler<TResult>((IRequest) request, token, out var handleTask))
                return handleTask;
            
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResult));

            var internalHandler = typeof(DynamicDispatchMediator)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .SingleOrDefault(x => x.Name == "DispatchAsync" && x.GetGenericArguments().Length == 2)
                .MakeGenericMethod(requestType, typeof(TResult));

            _handlerCache[requestType] = new HandlerCacheItem
            {
                HandlerMethod = internalHandler,
                RequestType = requestType,
                HandlerType = handlerType
            };
            
            return (Task<Response<TResult>>) internalHandler.Invoke(this, new object[] { request, handlerType, token });
        }

        private bool TryNakedHandler<TResult>(IRequest request, CancellationToken token, out Task<Response<TResult>> task)
        {
            try
            {
                var requestType = request.GetType();
                var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

                var internalHandler = typeof(DynamicDispatchMediator)
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .SingleOrDefault(x => x.Name == "DispatchAsync" && x.GetGenericArguments().Length == 1)
                    .MakeGenericMethod(requestType);

                task = (Task<Response<TResult>>) internalHandler.Invoke(this, new object[] { request, handlerType, token });

                _handlerCache[requestType] = new HandlerCacheItem
                {
                    HandlerMethod = internalHandler,
                    RequestType = requestType,
                    HandlerType = handlerType
                };
                
                return true;
            }
            catch
            {
                task = null;
                return false;
            }
        }

        private Task<Response<NoResult>> DispatchAsync<TRequest>(TRequest request, Type handlerType, CancellationToken token)
            where TRequest : IRequest
        {
            var handler = _resolver(handlerType) as IRequestHandler<TRequest>;
            
            return handler.HandleAsync(request, token).ContinueWith(t => (Response<NoResult>) t.Result);
        }

        private Task<Response<TResult>> DispatchAsync<TRequest, TResult>(TRequest request, Type handlerType, CancellationToken token)
            where TRequest : IRequest<TResult>
        {
            var handler = _resolver(handlerType) as IRequestHandler<TRequest, TResult>;
            
            return handler.HandleAsync(request, token);
        }

        private class HandlerCacheItem
        {
            public MethodInfo HandlerMethod { get; set; }

            public Type RequestType { get; set; }

            public Type HandlerType { get; set; }
        }
    }
}