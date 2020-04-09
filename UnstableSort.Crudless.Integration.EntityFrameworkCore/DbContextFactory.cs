using Microsoft.EntityFrameworkCore;
using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.Integration.EntityFrameworkCore
{
    public abstract class DbContextFactory
    {
        public abstract DbContext FromRequestType<TRequest>(IServiceProvider provider);

        public abstract DbContext FromEntityType<TEntity>(IServiceProvider provider);
    }

    public class DiDbContextFactory : DbContextFactory
    {
        public override DbContext FromEntityType<TEntity>(IServiceProvider provider)
        {
            return (DbContext)provider.ProvideInstance(typeof(DbContext));
        }

        public override DbContext FromRequestType<TRequest>(IServiceProvider provider)
        {
            return (DbContext)provider.ProvideInstance(typeof(DbContext));
        }
    }
}
