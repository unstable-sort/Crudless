using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnstableSort.Crudless.Mediator
{
    public interface IResponse
    {
        bool HasErrors { get; }

        List<Error> Errors { get; }
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

        public static implicit operator Response(Response<TResult> response)
            => new Response { Errors = response.Errors };
    }

    public class Response : Response<NoResult>
    {
        public static Response Success() => new Response();

        public static Task<Response> SuccessAsync() => Task.FromResult(Success());
    }
}