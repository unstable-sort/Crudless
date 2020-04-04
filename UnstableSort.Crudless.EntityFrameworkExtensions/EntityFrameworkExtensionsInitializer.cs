using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Context;
using UnstableSort.Crudless.EntityFrameworkExtensions.Configuration;
using Z.EntityFramework.Extensions;

namespace UnstableSort.Crudless.EntityFrameworkExtensions
{
    public enum BulkExtensions
    {
        None = 0,

        Create = 1 << 0,
        Update = 1 << 1,
        Delete = 1 << 2,

        All = Create | Update | Delete
    }

    public class EntityFrameworkExtensionsInitializer : CrudlessInitializationTask
    {
        private readonly BulkExtensions _extensions;

        public EntityFrameworkExtensionsInitializer(BulkExtensions extensions)
        {
            _extensions = extensions;
        }

        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            BulkConfigurationManager.Clear();

            using (container.AllowOverrides())
            {
                var bulkAgent = new BulkDataAgent();

                if ((_extensions & BulkExtensions.Create) == BulkExtensions.Create)
                    container.RegisterInstance<IBulkCreateDataAgent>(bulkAgent);

                if ((_extensions & BulkExtensions.Update) == BulkExtensions.Update)
                    container.RegisterInstance<IBulkUpdateDataAgent>(bulkAgent);

                if ((_extensions & BulkExtensions.Delete) == BulkExtensions.Delete)
                    container.RegisterInstance<IBulkDeleteDataAgent>(bulkAgent);
            }

            EntityFrameworkManager.ContextFactory = 
                context => container.CreateProvider().ProvideInstance<DbContext>();
        }

        public override bool Supports(string option)
        {
            if (option == "EntityFrameworkExtensions")
                return true;

            return base.Supports(option);
        }
    }
}
