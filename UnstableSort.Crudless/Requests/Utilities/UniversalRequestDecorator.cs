using System;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Requests
{
    public class UniversalRequestDecorator<TRequest>
        : IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        private readonly IRequestConfig _requestConfig;
        private readonly ServiceProviderContainer _container;
        private readonly Func<IRequestHandler<TRequest>> _decorateeFactory;

        public UniversalRequestDecorator(CrudlessConfigManager profileManager,
            ServiceProviderContainer container,
            Func<IRequestHandler<TRequest>> decorateeFactory)
        {
            _container = container;
            _decorateeFactory = decorateeFactory;
            _requestConfig = profileManager.GetRequestConfigFor<TRequest>();
        }

        public async Task<Response> HandleAsync(TRequest request, CancellationToken token)
        {
            using (var provider = _container.CreateProvider())
            {
                foreach (var requestHook in _requestConfig.GetRequestHooks(provider))
                {
                    await requestHook.Run(request, token).Configure();
                    token.ThrowIfCancellationRequested();
                }

                return await _decorateeFactory().HandleAsync(request, token);
            }
        }
    }

    public class UniversalRequestDecorator<TRequest, TResult>
        : IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        private readonly IRequestConfig _requestConfig;
        private readonly ServiceProviderContainer _container;
        private readonly Func<IRequestHandler<TRequest, TResult>> _decorateeFactory;

        public UniversalRequestDecorator(CrudlessConfigManager profileManager,
            ServiceProviderContainer container,
            Func<IRequestHandler<TRequest, TResult>> decorateeFactory)
        {
            _container = container;
            _decorateeFactory = decorateeFactory;
            _requestConfig = profileManager.GetRequestConfigFor<TRequest>();
        }

        public async Task<Response<TResult>> HandleAsync(TRequest request, CancellationToken token)
        {
            using (var provider = _container.CreateProvider())
            {
                foreach (var requestHook in _requestConfig.GetRequestHooks(provider))
                {
                    await requestHook.Run(request, token).Configure();
                    token.ThrowIfCancellationRequested();
                }

                var response = await _decorateeFactory().HandleAsync(request, token);

                if (response.HasErrors)
                    return response;

                var result = response.Result;

                foreach (var hook in _requestConfig.GetResultHooks(provider))
                {
                    if (typeof(TResult).IsAssignableFrom(hook.ResultType))
                        result = (TResult)await hook.Run(request, result, token).Configure();
                    else
                        result = await ResultHookAdapter.Adapt(hook, request, result, token).Configure();

                    token.ThrowIfCancellationRequested();
                }

                return result.AsResponse();
            }
        }
    }
}
