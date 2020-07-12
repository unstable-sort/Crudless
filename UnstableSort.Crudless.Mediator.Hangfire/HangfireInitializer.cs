using System.Reflection;
using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.Mediator.Hangfire
{
    public class HangfireInitializer : CrudlessInitializationTask
    {
        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            using (container.AllowOverrides())
            {
                container.Register<IBackgroundJobMediator, BackgroundJobMediator>();
                container.Register(() => new BackgroundJobContext(true));
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
