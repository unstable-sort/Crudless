namespace UnstableSort.Crudless.Mediator.Hangfire
{
    public abstract class BackgroundJobAdapter
    {
        public abstract IBackgroundJob<TResult> Adapt<TResult>(IRequest<TResult> request);
    }

    public sealed class SimpleBackgroundJobAdapter : BackgroundJobAdapter
    {
        public override IBackgroundJob<TResult> Adapt<TResult>(IRequest<TResult> request)
            => new SimpleBackgroundJob<TResult>(request);
    }

    public class SimpleBackgroundJob<TResult> : IBackgroundJob<TResult>
    {
        public SimpleBackgroundJob(IRequest<TResult> request)
        {
            Request = request;
        }

        public IRequest<TResult> Request { get; }
    }
}
