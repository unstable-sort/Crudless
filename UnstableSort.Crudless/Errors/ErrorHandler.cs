using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Errors
{
    public interface IErrorHandler
    {
        Response Handle(CrudlessError error);

        Response<TResult> Handle<TResult>(CrudlessError error);
    }

    public class ErrorHandler : IErrorHandler
    {
        public const string GenericErrorMessage = "An error occurred while processing the request.";
        public const string CanceledErrorMessage = "The request was canceled while processing.";

        private readonly Dictionary<Type, Func<CrudlessError, Response>> _dispatchers;

        public ErrorHandler()
        {
            _dispatchers = new Dictionary<Type, Func<CrudlessError, Response>>
            {
                { typeof(RequestFailedError), e => HandleError((RequestFailedError)e) },
                { typeof(FailedToFindError), e => HandleError((FailedToFindError)e) },
                { typeof(RequestCanceledError), e => HandleError((RequestCanceledError)e) },
                { typeof(HookFailedError), e => HandleError((HookFailedError)e) },
                { typeof(CreateEntityFailedError), e => HandleError((CreateEntityFailedError)e) },
                { typeof(UpdateEntityFailedError), e => HandleError((UpdateEntityFailedError)e) },
                { typeof(CreateResultFailedError), e => HandleError((CreateResultFailedError)e) }
            };
        }

        public Response Handle(CrudlessError error)
        {
            if (_dispatchers.TryGetValue(error.GetType(), out var dispatcher))
                return dispatcher(error);

            return HandleError(error);
        }

        public Response<TResult> Handle<TResult>(CrudlessError error)
        {
            var response = Handle(error);

            return new Response<TResult>
            {
                Errors = response.Errors.ToList(),
                Result = default(TResult),
                StatusCode = response.StatusCode
            };
        }

        protected virtual Response HandleError(CrudlessError error)
        {
            if (error.Exception != null)
                throw error.Exception;

            var response = GenericErrorMessage.AsError().AsResponse();
            response.StatusCode = HttpStatusCode.InternalServerError;

            return response;
        }
        
        protected virtual Response HandleError(RequestFailedError error)
            => error.Reason.AsError().AsResponse();
        
        protected virtual Response HandleError(RequestCanceledError error)
            => CanceledErrorMessage.AsError().AsResponse();

        protected virtual Response HandleError(FailedToFindError error)
            => error.Reason.AsError().AsResponse().WithStatusCode(HttpStatusCode.NotFound);

        protected virtual Response HandleError(HookFailedError error)
            => error.Reason.AsError().AsResponse().WithStatusCode(HttpStatusCode.InternalServerError);

        protected virtual Response HandleError(CreateEntityFailedError error)
            => error.Reason.AsError().AsResponse().WithStatusCode(HttpStatusCode.InternalServerError);

        protected virtual Response HandleError(UpdateEntityFailedError error)
            => error.Reason.AsError().AsResponse().WithStatusCode(HttpStatusCode.InternalServerError);

        protected virtual Response HandleError(CreateResultFailedError error)
            => error.Reason.AsError().AsResponse().WithStatusCode(HttpStatusCode.InternalServerError);
    }
}
