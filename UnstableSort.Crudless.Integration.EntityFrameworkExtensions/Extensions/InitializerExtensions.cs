using UnstableSort.Crudless.Integration.EntityFrameworkCore;
using UnstableSort.Crudless.Integration.EntityFrameworkExtensions;

namespace UnstableSort.Crudless
{
    public static class IncludeEntityFrameworkExtensionsInitializer
    {
        public static CrudlessInitializer UseEntityFrameworkExtensions(this CrudlessInitializer initializer, 
            BulkExtensions extensions = BulkExtensions.All)
        {
            EntityFrameworkCoreInitializer.Unregister(initializer);

            return initializer
                .UseEntityFramework()
                .AddInitializer(new EntityFrameworkExtensionsInitializer(extensions));
        }
    }
}
