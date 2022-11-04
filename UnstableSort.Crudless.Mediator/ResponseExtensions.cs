using System.Net;

namespace UnstableSort.Crudless.Mediator
{
    public static class ResponseExtensions
    {
        public static Response WithStatusCode(this Response response, HttpStatusCode statusCode)
        {
            response.StatusCode = statusCode;

            return response;
        }

        public static Response<TResult> WithStatusCode<TResult>(this Response<TResult> response, HttpStatusCode statusCode)
        {
            response.StatusCode = statusCode;

            return response;
        }
    }
}
