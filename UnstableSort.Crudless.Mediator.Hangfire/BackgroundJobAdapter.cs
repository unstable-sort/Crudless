namespace UnstableSort.Crudless.Mediator.Hangfire
{
    public abstract class BackgroundJobAdapter
    {
        public abstract IBackgroundJob<TRequest, TResult> Adapt<TRequest, TResult>(TRequest request)
            where TRequest : IRequest<TResult>;
    }

    public sealed class SimpleBackgroundJobAdapter : BackgroundJobAdapter
    {
        public override IBackgroundJob<TRequest, TResult> Adapt<TRequest, TResult>(TRequest request)
            => new SimpleBackgroundJob<TRequest, TResult>(request);
    }
}
