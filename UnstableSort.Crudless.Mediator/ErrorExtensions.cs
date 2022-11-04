using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace UnstableSort.Crudless.Mediator
{
    public static class ErrorExtensions
    {
        public static Response AsResponse(this Error error)
            => new Response { Errors = new List<Error> { error } };

        public static Response<TResult> AsResponse<TResult>(this Error error)
            => new Response<TResult> { Errors = new List<Error> { error } };

        public static Task<Response> AsResponseAsync(this Error error)
            => Task.FromResult(new Response { Errors = new List<Error> { error } });

        public static Task<Response<TResult>> AsResponseAsync<TResult>(this Error error)
            => Task.FromResult(new Response<TResult> { Errors = new List<Error> { error } });

        public static Response AsResponse(this List<Error> errors)
            => new Response { Errors = errors.ToList() };

        public static Response<TResult> AsResponse<TResult>(this List<Error> errors)
            => new Response<TResult> { Errors = errors.ToList() };

        public static Task<Response> AsResponseAsync(this List<Error> errors)
            => Task.FromResult(new Response { Errors = errors.ToList() });

        public static Task<Response<TResult>> AsResponseAsync<TResult>(this List<Error> errors)
            => Task.FromResult(new Response<TResult> { Errors = errors.ToList() });

        public static Response<TResult> AsBadRequest<TResult>(this Response<TResult> response)
            => new Response<TResult> { Errors = response.Errors, StatusCode = HttpStatusCode.BadRequest };

        public static Error AsError(this string message)
            => new Error { ErrorMessage = message };
    }
}
