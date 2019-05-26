using UnstableSort.Crudless.EntityFrameworkCore;

namespace UnstableSort.Crudless
{
    public static class IncludeEntityFrameworkCoreInitializer
    {
        public static CrudlessInitializer UseEntityFramework(this CrudlessInitializer initializer)
        {
            return initializer.AddInitializer(new EntityFrameworkCoreInitializer());
        }
    }
}
