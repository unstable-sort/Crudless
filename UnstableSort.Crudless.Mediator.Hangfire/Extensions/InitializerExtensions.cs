using UnstableSort.Crudless.Mediator.Hangfire;

namespace UnstableSort.Crudless
{
    public static class IncludeHangfireInitializer
    {
        public static CrudlessInitializer UseBackgroundMediator(this CrudlessInitializer initializer)
        {
            return initializer.AddInitializer(new HangfireInitializer());
        }
    }
}
