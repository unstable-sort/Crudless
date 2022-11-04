namespace UnstableSort.Crudless.Mediator.Hangfire
{
    public interface IBackgroundJob<TResult>
    {
        IRequest<TResult> Request { get; }
    }
}
