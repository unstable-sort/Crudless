using System;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Requests
{
    public class UniversalRequestDecorator<TRequest>
        : IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        private readonly IRequestConfig _requestConfig;
        private readonly Func<IRequestHandler<TRequest>> _decorateeFactory;

        public UniversalRequestDecorator(CrudlessConfigManager profileManager,
            Func<IRequestHandler<TRequest>> decorateeFactory)
        {
            _decorateeFactory = decorateeFactory;
            _requestConfig = profileManager.GetRequestConfigFor<TRequest>();
        }

        public async Task<Response> HandleAsync(TRequest request, CancellationToken token)
        {
            foreach (var requestHook in _requestConfig.GetRequestHooks())
            {
                await requestHook.Run(request, token).Configure();
                token.ThrowIfCancellationRequested();
            }

            return await _decorateeFactory().HandleAsync(request, token);
        }
    }

    public class UniversalRequestDecorator<TRequest, TResult>
        : IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        private readonly IRequestConfig _requestConfig;
        private readonly Func<IRequestHandler<TRequest, TResult>> _decorateeFactory;

        public UniversalRequestDecorator(CrudlessConfigManager profileManager,
            Func<IRequestHandler<TRequest, TResult>> decorateeFactory)
        {
            _decorateeFactory = decorateeFactory;
            _requestConfig = profileManager.GetRequestConfigFor<TRequest>();
        }

        public async Task<Response<TResult>> HandleAsync(TRequest request, CancellationToken token)
        {
            foreach (var requestHook in _requestConfig.GetRequestHooks())
            {
                await requestHook.Run(request, token).Configure();
                token.ThrowIfCancellationRequested();
            }

            var response = await _decorateeFactory().HandleAsync(request, token);

            if (response.HasErrors)
                return response;

            var result = response.Result;

            foreach (var hook in _requestConfig.GetResultHooks())
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
