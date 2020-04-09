using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Context;
using UnstableSort.Crudless.Errors;
using UnstableSort.Crudless.Mediator;
using IServiceProvider = UnstableSort.Crudless.Common.ServiceProvider.IServiceProvider;

namespace UnstableSort.Crudless.Requests
{
    internal abstract class CrudlessRequestHandler<TRequest, TEntity> : ICrudlessRequestHandler
        where TEntity : class
    {
        protected readonly IEntityContext Context;
        protected readonly DataContext<TEntity> DataContext;
        
        protected CrudlessRequestHandler(IEntityContext context, CrudlessConfigManager profileManager)
        {
            Context = context;
            RequestConfig = profileManager.GetRequestConfigFor<TRequest>();
            DataContext = new DataContext<TEntity>(RequestConfig);

            var errorHandler = RequestConfig.ErrorConfig.GetErrorHandlerFor<TEntity>();
            ErrorDispatcher = new ErrorDispatcher(errorHandler);
        }

        protected IRequestConfig RequestConfig { get; private set; }

        protected void ApplyConfiguration(TRequest request)
        {
            var requestType = request.GetType();

            if (typeof(IInlineConfiguredRequest).IsAssignableFrom(requestType) ||
                typeof(IInlineConfiguredBulkRequest).IsAssignableFrom(requestType))
            {
                try
                {
                    var buildProfileMethod = requestType.GetMethod("BuildProfile", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                    var profile = buildProfileMethod.Invoke(request, Array.Empty<object>());

                    if (profile != null)
                    {
                        var profileType = profile.GetType();
                        var applyMethod = profileType
                            .GetMethod(nameof(RequestProfile.Apply), BindingFlags.NonPublic | BindingFlags.Instance)
                            .MakeGenericMethod(requestType);

                        applyMethod.Invoke(profile, new object[] { RequestConfig });
                    }
                } 
                catch (TargetInvocationException e)
                {
                    if (e.InnerException != null)
                        throw e.InnerException;

                    throw e;
                }
            }
        }

        protected async Task<Response> HandleWithErrorsAsync(TRequest request,
            IServiceProvider provider,
            CancellationToken token, 
            Func<TRequest, IServiceProvider, CancellationToken, Task> handleAsync)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    await handleAsync(request, provider, token).Configure();
                }
                catch (AggregateException e)
                {
                    throw Unwrap(e);
                }
            }
            catch (Exception e) when (FailedToFindError.IsReturnedFor(e))
            {
                return ErrorDispatcher.Dispatch(FailedToFindError.From(request, e));
            }
            catch (Exception e) when (RequestFailedError.IsReturnedFor(e))
            {
                return ErrorDispatcher.Dispatch(RequestFailedError.From(request, e));
            }
            catch (Exception e) when (RequestCanceledError.IsReturnedFor(e))
            {
                return ErrorDispatcher.Dispatch(RequestCanceledError.From(request, e));
            }
            catch (Exception e) when (HookFailedError.IsReturnedFor(e))
            {
                return ErrorDispatcher.Dispatch(HookFailedError.From(request, e));
            }
            catch (Exception e) when (CreateEntityFailedError.IsReturnedFor(e))
            {
                return ErrorDispatcher.Dispatch(CreateEntityFailedError.From(request, e));
            }
            catch (Exception e) when (UpdateEntityFailedError.IsReturnedFor(e))
            {
                return ErrorDispatcher.Dispatch(UpdateEntityFailedError.From(request, e));
            }
            catch (Exception e) when (CreateResultFailedError.IsReturnedFor(e))
            {
                return ErrorDispatcher.Dispatch(CreateResultFailedError.From(request, e));
            }

            return Response.Success();
        }

        protected async Task<Response<TResult>> HandleWithErrorsAsync<TResult>(TRequest request,
            IServiceProvider provider,
            CancellationToken token, 
            Func<TRequest, IServiceProvider, CancellationToken, Task<TResult>> handleAsync)
        {
            var result = default(TResult);

            try
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    result = await handleAsync(request, provider, token).Configure();
                }
                catch (AggregateException e)
                {
                    throw Unwrap(e);
                }
            }
            catch (Exception e) when (FailedToFindError.IsReturnedFor(e))
            {
                return ErrorDispatcher.Dispatch<TResult>(FailedToFindError.From(request, e));
            }
            catch (Exception e) when (RequestFailedError.IsReturnedFor(e))
            {
                return ErrorDispatcher.Dispatch<TResult>(RequestFailedError.From(request, e));
            }
            catch (Exception e) when (RequestCanceledError.IsReturnedFor(e))
            {
                return ErrorDispatcher.Dispatch<TResult>(RequestCanceledError.From(request, e));
            }
            catch (Exception e) when (HookFailedError.IsReturnedFor(e))
            {
                return ErrorDispatcher.Dispatch<TResult>(HookFailedError.From(request, e));
            }
            catch (Exception e) when (CreateEntityFailedError.IsReturnedFor(e))
            {
                return ErrorDispatcher.Dispatch<TResult>(CreateEntityFailedError.From(request, e));
            }
            catch (Exception e) when (UpdateEntityFailedError.IsReturnedFor(e))
            {
                return ErrorDispatcher.Dispatch<TResult>(UpdateEntityFailedError.From(request, e));
            }
            catch (Exception e) when (CreateResultFailedError.IsReturnedFor(e))
            {
                return ErrorDispatcher.Dispatch<TResult>(CreateResultFailedError.From(request, e));
            }

            return result.AsResponse();
        }

        private Exception Unwrap(AggregateException e)
        {
            Exception inner = e;

            if (inner.InnerException == null)
                throw e;

            while (inner.InnerException != null && inner.InnerException is AggregateException)
                inner = inner.InnerException;

            return inner;
        }

        public ErrorDispatcher ErrorDispatcher { get; }
    }
}
