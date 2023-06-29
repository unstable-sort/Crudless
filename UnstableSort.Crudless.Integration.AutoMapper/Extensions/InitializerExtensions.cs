using AutoMapper;
using UnstableSort.Crudless.Integration.AutoMapper;

namespace UnstableSort.Crudless
{
    public static class IncludeAutoMapperInitializer
    {
        public static CrudlessInitializer UseAutoMapper(this CrudlessInitializer initializer) 
            => initializer.AddInitializer(new AutoMapperInitializer());

        public static CrudlessInitializer UseAutoMapper(this CrudlessInitializer initializer, IMapper instance)
            => initializer.AddInitializer(new AutoMapperInitializer(instance));
    }
}
