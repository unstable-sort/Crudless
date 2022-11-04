using System;
using System.Reflection;
using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.Mediator.Hangfire
{
    public class HangfireInitializer : CrudlessInitializationTask
    {
        private readonly CrudlessHangfireOptions _options;

        public HangfireInitializer(CrudlessHangfireOptions options)
        {
            _options = options;
        }

        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            if (_options.BackgroundJobAdapterType == null)
                throw new ArgumentNullException(nameof(_options.BackgroundJobAdapterType), 
                    "Job Adapter Type may not be null. Use the default adapter (SimpleBackgroundJobAdapter) or implement your own.");

            if (_options.BackgroundJobExecutorType == null)
                throw new ArgumentNullException(nameof(_options.BackgroundJobExecutorType),
                    "Job Executor Type may not be null. Use the default adapter (SimpleBackgroundJobExecutor) or implement your own.");

            using (container.AllowOverrides())
            {
                container.Register<IBackgroundJobMediator, BackgroundJobMediator>();
                container.Register(() => new BackgroundJobContext(true));
                container.Register(typeof(BackgroundJobAdapter), _options.BackgroundJobAdapterType);
                container.Register(typeof(BackgroundJobExecutor<,>), _options.BackgroundJobExecutorType);
            }
        }

        public override bool Supports(string option)
        {
            if (option == "BackgroundMediator")
                return true;

            return base.Supports(option);
        }
    }
}
