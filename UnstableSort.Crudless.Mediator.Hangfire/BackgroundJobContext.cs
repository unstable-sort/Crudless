namespace UnstableSort.Crudless.Mediator.Hangfire
{
    public class BackgroundJobContext
    {
        public bool IsBackgroundJob { get; }

        public BackgroundJobContext(bool isBackgroundJob)
        {
            IsBackgroundJob = isBackgroundJob;
        }
    }
}
