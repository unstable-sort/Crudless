using System;
using UnstableSort.Crudless.Mediator.Hangfire;

namespace UnstableSort.Crudless
{
    public static class IncludeHangfireInitializer
    {
        public static CrudlessInitializer UseBackgroundMediator(this CrudlessInitializer initializer)
            => UseBackgroundMediator(initializer, _ => { });
        
        public static CrudlessInitializer UseBackgroundMediator(this CrudlessInitializer initializer,
            Action<CrudlessHangfireOptions> configure)
        {
            var options = new CrudlessHangfireOptions();

            configure(options);

            return initializer.AddInitializer(new HangfireInitializer(options));
        }
    }
}
