﻿using System;
using System.Reflection;
using SimpleInjector;
using UnstableSort.Crudless.Context;

namespace UnstableSort.Crudless.EntityFrameworkCore
{
    public class EntityFrameworkCoreInitializer : CrudlessInitializationTask
    {
        internal EntityFrameworkCoreInitializer(Type dbContextFactoryType)
        {
            DbContextFactoryType = dbContextFactoryType;
        }

        public Type DbContextFactoryType { get; }

        public override void Run(Container container, Assembly[] assemblies, CrudlessOptions options)
        {
            DiDbContextFactory.BindContainer(container.GetInstance);

            container.Register(typeof(DbContextFactory), DbContextFactoryType, Lifestyle.Scoped);

            container.Register<IEntityContext, EntityFrameworkContext>(Lifestyle.Scoped);

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
