using System;
using UnstableSort.Crudless.Integration.EntityFrameworkCore;

namespace UnstableSort.Crudless
{
    public static class IncludeEntityFrameworkCoreInitializer
    {
        public static CrudlessInitializer UseEntityFramework(this CrudlessInitializer initializer, Type dbContextFactoryType = null)
        {
            Type contextFactoryType = dbContextFactoryType ?? typeof(DiDbContextFactory);
            if (!typeof(DbContextFactory).IsAssignableFrom(contextFactoryType))
                throw new ArgumentException($"'{contextFactoryType}' does not inherit DbContextFactory", nameof(dbContextFactoryType));

            return initializer.AddInitializer(new EntityFrameworkCoreInitializer(contextFactoryType));
        }
    }
}
