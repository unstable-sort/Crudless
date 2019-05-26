using System.Threading.Tasks;

namespace UnstableSort.Crudless.Mediator
{
    public class NoResult
    {
        public static Response<NoResult> AsResponse() => new Response<NoResult>();

        public static Task<Response<NoResult>> AsResponseAsync() => Task.FromResult(AsResponse());
    }
}
