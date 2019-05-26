using System;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Context;
using UnstableSort.Crudless.Errors;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Requests
{
    internal abstract class CrudlessRequestHandler<TRequest, TEntity> : ICrudlessRequestHandler
        where TEntity : class
    {
        protected readonly IEntityContext Context;
        protected readonly IRequestConfig RequestConfig;
        protected readonly DataContext<TEntity> DataContext;
        
        protected CrudlessRequestHandler(IEntityContext context, CrudlessConfigManager profileManager)
        {
            Context = context;
            RequestConfig = profileManager.GetRequestConfigFor<TRequest>();
            DataContext = new DataContext<TEntity>(RequestConfig);

            var errorHandler = RequestConfig.ErrorConfig.GetErrorHandlerFor<TEntity>();
            ErrorDispatcher = new ErrorDispatcher(errorHandler);
        }

        protected async Task<Response> HandleWithErrorsAsync(TRequest request, 
            CancellationToken token, 
            Func<TRequest, CancellationToken, Task> handleAsync)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                await handleAsync(request, token).Configure();
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
            CancellationToken token, 
            Func<TRequest, CancellationToken, Task<TResult>> handleAsync)
        {
            var result = default(TResult);

            try
            {
                token.ThrowIfCancellationRequested();

                result = await handleAsync(request, token).Configure();
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

        public ErrorDispatcher ErrorDispatcher { get; }
    }
}
