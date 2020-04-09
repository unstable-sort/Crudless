using System;
using System.Reflection;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Context;

namespace UnstableSort.Crudless.Integration.EntityFrameworkCore
{
    public class EntityFrameworkCoreInitializer : CrudlessInitializationTask
    {
        internal EntityFrameworkCoreInitializer(Type dbContextFactoryType)
        {
            DbContextFactoryType = dbContextFactoryType;
        }

        public Type DbContextFactoryType { get; }

        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            container.RegisterScoped(typeof(DbContextFactory), DbContextFactoryType);

            container.RegisterScoped<IEntityContext, EntityFrameworkContext>();

            var dataAgent = new EntityFrameworkDataAgent();
            container.RegisterInstance<ICreateDataAgent>(dataAgent);
            container.RegisterInstance<IUpdateDataAgent>(dataAgent);
            container.RegisterInstance<IDeleteDataAgent>(dataAgent);
            container.RegisterInstance<IBulkCreateDataAgent>(dataAgent);
            container.RegisterInstance<IBulkUpdateDataAgent>(dataAgent);
            container.RegisterInstance<IBulkDeleteDataAgent>(dataAgent);
        }

        public override bool Supports(string option)
        {
            if (option == "EntityFramework" || option == "EntityFrameworkCore")
                return true;

            return base.Supports(option);
        }

        public static void Unregister(CrudlessInitializer initializer)
        {
            initializer.RemoveInitializers<EntityFrameworkCoreInitializer>();
        }
    }
}
