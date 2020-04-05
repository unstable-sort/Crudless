using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Exceptions;

namespace UnstableSort.Crudless.Configuration
{
    public abstract class UniversalRequestProfile<TRequest>
        : RequestProfile
    {
        private List<RequestProfile> _inheritProfiles
            = new List<RequestProfile>();

        protected internal readonly List<IRequestHookFactory> RequestHooks
            = new List<IRequestHookFactory>();

        protected internal readonly List<IResultHookFactory> ResultHooks
            = new List<IResultHookFactory>();

        public override Type RequestType => typeof(TRequest);

        internal override void Inherit(IEnumerable<RequestProfile> profiles)
        {
            _inheritProfiles = profiles.ToList();
        }

        internal override IRequestConfig BuildConfiguration()
        {
            var config = (RequestConfig<TRequest>)Activator.CreateInstance(
                typeof(RequestConfig<>).MakeGenericType(typeof(TRequest)));

            foreach (var profile in _inheritProfiles)
                profile.Apply(config);

            return config;
        }

        internal override void Apply<TConfigRequest>(RequestConfig<TConfigRequest> config)
        {
            config.AddRequestHooks(RequestHooks);
            config.AddResultHooks(ResultHooks);
        }

        protected void AddRequestHook<THook, TBaseRequest>()
            where THook : RequestHook<TBaseRequest>
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddRequestHook), typeof(TBaseRequest), typeof(TRequest));

            RequestHooks.Add(TypeRequestHookFactory.From<THook, TBaseRequest>());
        }

        protected void AddRequestHook<THook>()
            where THook : RequestHook<TRequest>
            => AddRequestHook<THook, TRequest>();

        protected void AddRequestHook<TBaseRequest>(RequestHook<TBaseRequest> hook)
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddRequestHook), typeof(TBaseRequest), typeof(TRequest));

            RequestHooks.Add(InstanceRequestHookFactory.From(hook));
        }

        protected void AddRequestHook(Func<TRequest, CancellationToken, Task> hook)
        {
            RequestHooks.Add(FunctionRequestHookFactory.From(hook));
        }

        protected void AddRequestHook(Func<TRequest, Task> hook)
            => AddRequestHook((request, ct) => hook(request));

        protected void AddRequestHook(Action<TRequest> hook)
        {
            RequestHooks.Add(FunctionRequestHookFactory.From(hook));
        }

        protected void AddResultHook<THook, TBaseRequest, TResult>()
            where THook : ResultHook<TBaseRequest, TResult>
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddResultHook), typeof(TBaseRequest), typeof(TRequest));

            ResultHooks.Add(TypeResultHookFactory.From<THook, TBaseRequest, TResult>());
        }

        protected void AddResultHook<THook, TResult>()
            where THook : ResultHook<TRequest, TResult>
            => AddResultHook<THook, TRequest, TResult>();

        protected void AddResultHook<TBaseRequest, TResult>(ResultHook<TBaseRequest, TResult> hook)
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddResultHook), typeof(TBaseRequest), typeof(TRequest));

            ResultHooks.Add(InstanceResultHookFactory.From(hook));
        }

        protected void AddResultHook<TResult>(Func<TRequest, TResult, CancellationToken, Task<TResult>> hook)
        {
            ResultHooks.Add(FunctionResultHookFactory.From(hook));
        }

        protected void AddResultHook<TResult>(Func<TRequest, TResult, Task<TResult>> hook)
            => AddResultHook<TResult>((request, result, ct) => hook(request, result));

        protected void AddResultHook<TResult>(Func<TRequest, TResult, TResult> hook)
        {
            ResultHooks.Add(FunctionResultHookFactory.From(hook));
        }
    }

    public class DefaultUniversalRequestProfile<TRequest> : UniversalRequestProfile<TRequest>
    {
    }
}
