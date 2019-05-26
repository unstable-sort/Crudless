using System.Reflection;
using SimpleInjector;
using UnstableSort.Crudless.Context;

namespace UnstableSort.Crudless.EntityFrameworkCore
{
    public class EntityFrameworkCoreInitializer : ICrudlessInitializationTask
    {
        public void Run(Container container, Assembly[] assemblies, CrudlessOptions options)
        {
            container.Register<IEntityContext, EntityFrameworkContext>(Lifestyle.Scoped);

            var dataAgent = new EntityFrameworkDataAgent();
            container.RegisterInstance<ICreateDataAgent>(dataAgent);
            container.RegisterInstance<IUpdateDataAgent>(dataAgent);
            container.RegisterInstance<IDeleteDataAgent>(dataAgent);
            container.RegisterInstance<IBulkCreateDataAgent>(dataAgent);
            container.RegisterInstance<IBulkUpdateDataAgent>(dataAgent);
            container.RegisterInstance<IBulkDeleteDataAgent>(dataAgent);
        }

        public static void Unregister(CrudlessInitializer initializer)
        {
            initializer.RemoveInitializers<EntityFrameworkCoreInitializer>();
        }
    }
}
