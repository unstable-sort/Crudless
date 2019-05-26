using System.Reflection;
using SimpleInjector;
using UnstableSort.Crudless.Context;

namespace UnstableSort.Crudless.Tests.Utilities
{
    public class SoftDeleteInitializer : ICrudlessInitializationTask
    {
        public void Run(Container container, Assembly[] assemblies, CrudlessOptions options)
        {
            container.Options.AllowOverridingRegistrations = true;

            container.Register<IDeleteDataAgent, SoftDeleteDataAgent>(Lifestyle.Singleton);
            container.Register<IBulkDeleteDataAgent, SoftDeleteDataAgent>(Lifestyle.Singleton);

            container.Options.AllowOverridingRegistrations = false;
        }
    }
}
