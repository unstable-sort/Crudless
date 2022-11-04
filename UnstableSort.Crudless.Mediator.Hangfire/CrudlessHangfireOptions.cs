using System;

namespace UnstableSort.Crudless.Mediator.Hangfire
{
    public class CrudlessHangfireOptions
    {
        public Type BackgroundJobAdapterType { get; set; } = typeof(SimpleBackgroundJobAdapter);

        public Type BackgroundJobExecutorType { get; set; } = typeof(SimpleBackgroundJobExecutor<,>);
    }
}
