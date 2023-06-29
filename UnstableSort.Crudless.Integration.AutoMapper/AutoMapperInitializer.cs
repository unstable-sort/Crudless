using System.Reflection;
using AutoMapper;
using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.Integration.AutoMapper
{
    public class AutoMapperInitializer : CrudlessInitializationTask
    {
        private readonly IMapper _instance;

        public AutoMapperInitializer(IMapper instance = null)
        {
            _instance = instance;
        }

        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            using (var scope = container.AllowOverrides())
            {
                if (_instance != null)
                    container.RegisterInstance<IObjectMapper>(new AutoMapperObjectMapper(_instance));
                else
                    container.Register<IObjectMapper, AutoMapperObjectMapper>();
            }
        }

        public override bool Supports(string option)
        {
            if (option == "AutoMapper")
                return true;

            return base.Supports(option);
        }
    }
}