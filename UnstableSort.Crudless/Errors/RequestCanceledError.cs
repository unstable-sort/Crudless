using System;

namespace UnstableSort.Crudless.Errors
{
    public class RequestCanceledError : CrudlessError
    {
        public RequestCanceledError(object request, Exception exception = null)
            : base(exception)
        {
            Request = request;
        }

        public static bool IsReturnedFor(Exception e)
            => e is OperationCanceledException;

        public static RequestCanceledError From(object request, Exception exception)
            => new RequestCanceledError(request, exception);

        public object Request { get; }
    }
}
