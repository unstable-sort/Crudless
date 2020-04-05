using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        /// <summary>
        /// Adds a request hook of the given type.
        /// The hook will be resolved through the service provider.
        /// </summary>
        public void AddRequestHook(Type hookType)
        {
            var baseHookType = hookType
                .GetBaseTypes()
                .SingleOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(RequestHook<>));

            if (baseHookType == null)
                throw new ArgumentException($"Unable to add '{hookType}' as a request hook for '{typeof(TRequest)}'.\r\n" +
                                            $"Request hooks must inherit RequestHook<TRequest>.");

            var requestType = baseHookType.GenericTypeArguments[0];
            if (!requestType.IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddRequestHook), requestType, typeof(TRequest));

            var factoryMethod = typeof(TypeRequestHookFactory)
                .GetMethod(nameof(TypeRequestHookFactory.From), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(hookType, requestType);

            try
            {
                AddRequestHook((IRequestHookFactory)factoryMethod.Invoke(null, Array.Empty<object>()));
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw e;
            }
        }

        /// <summary>
        /// Adds a request hook instance.
        /// </summary>
        public void AddRequestHook(IRequestHook hook)
        {
            var hookType = hook.GetType();

            var baseHookType = hookType
                .GetBaseTypes()
                .SingleOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(RequestHook<>));

            if (baseHookType == null)
                throw new ArgumentException($"Unable to add '{hookType}' as a request hook for '{typeof(TRequest)}'.\r\n" +
                                            $"Request hooks must inherit RequestHook<TRequest>.");

            var requestType = baseHookType.GenericTypeArguments[0];
            if (!requestType.IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddRequestHook), requestType, typeof(TRequest));

            var factoryMethod = typeof(InstanceRequestHookFactory)
                .GetMethod(nameof(InstanceRequestHookFactory.From), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(requestType);

            try
            {
                AddRequestHook((IRequestHookFactory)factoryMethod.Invoke(null, new object[] { hook }));
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw e;
            }
        }

        /// <summary>
        /// Adds a request hook of the given type.
        /// The hook will be resolved through the service provider.
        /// </summary>
        public void AddRequestHook<THook>()
            where THook : IRequestHook
                => AddRequestHook(typeof(THook));

        /// <summary>
        /// Adds a request hook from the provided method.
        /// </summary>
        public void AddRequestHook(Func<TRequest, CancellationToken, Task> hook)
            => AddRequestHook(FunctionRequestHookFactory.From(hook));

        /// <summary>
        /// Adds a request hook from the provided method.
        /// </summary>
        public void AddRequestHook(Func<TRequest, Task> hook)
            => AddRequestHook((request, ct) => hook(request));

        /// <summary>
        /// Adds a request hook from the provided method.
        /// </summary>
        public void AddRequestHook(Action<TRequest> hook)
            => AddRequestHook(FunctionRequestHookFactory.From(hook));

        /// <summary>
        /// Adds a result hook of the given type.
        /// The hook will be resolved through the service provider.
        /// </summary>
        public void AddResultHook(Type hookType)
        {
            var baseHookType = hookType
                .GetBaseTypes()
                .SingleOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ResultHook<,>));

            if (baseHookType == null)
                throw new ArgumentException($"Unable to add '{hookType}' as a result hook for '{typeof(TRequest)}'.\r\n" +
                                            $"Result hooks must inherit ResultHook<TRequest, TResult>.");

            var requestType = baseHookType.GenericTypeArguments[0];
            if (!requestType.IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddResultHook), requestType, typeof(TRequest));

            var resultType = baseHookType.GenericTypeArguments[1];

            var factoryMethod = typeof(TypeResultHookFactory)
                .GetMethod(nameof(TypeResultHookFactory.From), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(hookType, requestType, resultType);

            try
            {
                AddResultHook((IResultHookFactory)factoryMethod.Invoke(null, Array.Empty<object>()));
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw e;
            }
        }

        /// <summary>
        /// Adds a result hook instance.
        /// </summary>
        public void AddResultHook(IResultHook hook)
        {
            var hookType = hook.GetType();

            var baseHookType = hookType
                .GetBaseTypes()
                .SingleOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ResultHook<,>));

            if (baseHookType == null)
                throw new ArgumentException($"Unable to add '{hookType}' as a result hook for '{typeof(TRequest)}'.\r\n" +
                                            $"Result hooks must inherit ResultHook<TRequest, TResult>.");

            var requestType = baseHookType.GenericTypeArguments[0];
            if (!requestType.IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddResultHook), requestType, typeof(TRequest));

            var resultType = baseHookType.GenericTypeArguments[1];

            var factoryMethod = typeof(InstanceResultHookFactory)
                .GetMethod(nameof(InstanceResultHookFactory.From), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(requestType, resultType);

            try
            {
                AddResultHook((IResultHookFactory)factoryMethod.Invoke(null, new object[] { hook }));
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                    throw e.InnerException;

                throw e;
            }
        }

        /// <summary>
        /// Adds a result hook of the given type.
        /// The hook will be resolved through the service provider.
        /// </summary>
        public void AddResultHook<THook>()
            where THook : IResultHook
                => AddResultHook(typeof(THook));

        /// <summary>
        /// Adds a result hook from the provided method.
        /// </summary>
        public void AddResultHook<TResult>(Func<TRequest, TResult, CancellationToken, Task<TResult>> hook)
            => AddResultHook(FunctionResultHookFactory.From(hook));

        /// <summary>
        /// Adds a result hook from the provided method.
        /// </summary>
        public void AddResultHook<TResult>(Func<TRequest, TResult, Task<TResult>> hook)
            => AddResultHook(FunctionResultHookFactory.From(hook));

        /// <summary>
        /// Adds a result hook from the provided method.
        /// </summary>
        public void AddResultHook<TResult>(Func<TRequest, TResult, TResult> hook)
             => AddResultHook(FunctionResultHookFactory.From(hook));

        /// <summary>
        /// Adds a result hook from the provided method.
        /// </summary>
        public void AddResultHook<TResult>(Func<TResult, TResult> hook)
             => AddResultHook<TResult>((_, result) => hook(result));

        internal void AddRequestHook(IRequestHookFactory requestHookFactory)
        {
            RequestHooks.Add(requestHookFactory);
        }

        internal void AddResultHook(IResultHookFactory resultHookFactory)
        {
            ResultHooks.Add(resultHookFactory);
        }
    }

    public class DefaultUniversalRequestProfile<TRequest> : UniversalRequestProfile<TRequest>
    {
    }
}
