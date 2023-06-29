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
                container.Register(() => new BackgroundJobContext(true));
                container.Register<IBackgroundMediator, BackgroundMediator>();
                container.Register(typeof(BackgroundJobAdapter), _options.BackgroundJobAdapterType);
                container.Register(_options.BackgroundJobExecutorType, _options.BackgroundJobExecutorType);
                container.RegisterInstance(_options);
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
