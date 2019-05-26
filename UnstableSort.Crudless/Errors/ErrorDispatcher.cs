using System;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Errors
{
    public class ErrorDispatcher
    {
        public IErrorHandler Handler { get; internal set; }

        public ErrorDispatcher(IErrorHandler handler)
        {
            Handler = handler;
        }

        public Response Dispatch(CrudlessError error)
        {
            return Handler.Handle(error);
        }

        public Response<TResult> Dispatch<TResult>(CrudlessError error)
        {
            return Handler.Handle<TResult>(error);
        }

        public Response Dispatch(Exception exception)
        {
            return Handler.Handle(new CrudlessError(exception));
        }

        public Response<TResult> Dispatch<TResult>(Exception exception)
        {
            return Handler.Handle<TResult>(new CrudlessError(exception));
        }
    }
}