namespace UnstableSort.Crudless.Mediator.Hangfire
{
    public interface IBackgroundJob<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        TRequest Request { get; }
    }

    public class SimpleBackgroundJob<TRequest, TResult> : IBackgroundJob<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        public SimpleBackgroundJob(TRequest request)
        {
            Request = request;
        }

        public TRequest Request { get; }
    }
}
