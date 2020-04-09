using System.Reflection;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Context;

namespace UnstableSort.Crudless.Tests.Utilities
{
    public class SoftDeleteInitializer : CrudlessInitializationTask
    {
        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            using (var scope = container.AllowOverrides())
            {
                container.RegisterSingleton<IDeleteDataAgent, SoftDeleteDataAgent>();
                container.RegisterSingleton<IBulkDeleteDataAgent, SoftDeleteDataAgent>();
            }
        }
    }
}
