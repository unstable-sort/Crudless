using System.Threading.Tasks;

namespace UnstableSort.Crudless.Mediator
{
    public static class ResultExtensions
    {
        public static Response<TResult> AsResponse<TResult>(this TResult item)
        {
            return new Response<TResult> { Result = item };
        }

        public static Task<Response<TResult>> AsResponseAsync<TResult>(this TResult item)
        {
            return Task.FromResult(AsResponse(item));
        }
    }
}
