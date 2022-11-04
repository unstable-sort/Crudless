using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace UnstableSort.Crudless.Mediator
{
    public interface IResponse
    {
        bool HasErrors { get; }

        List<Error> Errors { get; }

        HttpStatusCode StatusCode { get; }
    }

    public interface IResponse<TResult> : IResponse
    {
        TResult Result { get; }
    }

    public class Response<TResult> : IResponse<TResult>
    {
        public bool HasErrors => Errors.Any();

        public List<Error> Errors { get; set; } = new List<Error>();

        public TResult Result { get; set; }

        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        public static implicit operator Response(Response<TResult> response)
            => new Response { Errors = response.Errors, StatusCode = response.StatusCode };

        public static Response<TResult> Created() => new Response<TResult> { StatusCode = HttpStatusCode.Created };

        public static Response<TResult> Created(TResult result) => new Response<TResult> { Result = result, StatusCode = HttpStatusCode.Created };

        public static Response<TResult> Forbidden() => new Response<TResult> { StatusCode = HttpStatusCode.Forbidden };

        public static Response<TResult> NotFound() => new Response<TResult> { StatusCode = HttpStatusCode.NotFound };

        public static Response<TResult> Conflict() => new Response<TResult> { StatusCode = HttpStatusCode.Conflict };

        public static Response<TResult> BadRequest() => new Response<TResult> { StatusCode = HttpStatusCode.BadRequest };
    }

    public class Response : Response<NoResult>
    {
        public static Response Success() => new Response();

        public static Task<Response> SuccessAsync() => Task.FromResult(Success());
    }
}