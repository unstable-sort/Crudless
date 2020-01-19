using System;
using Microsoft.EntityFrameworkCore;

namespace UnstableSort.Crudless.EntityFrameworkCore
{
    public abstract class DbContextFactory
    {
        public abstract DbContext FromRequestType<TRequest>();

        public abstract DbContext FromEntityType<TEntity>();
    }

    public class DiDbContextFactory : DbContextFactory
    {
        private static Func<Type, object> s_serviceFactory;

        internal static void BindContainer(Func<Type, object> serviceFactory)
        {
            s_serviceFactory = serviceFactory;
        }

        public override DbContext FromEntityType<TEntity>()
            => (DbContext)s_serviceFactory(typeof(DbContext));

        public override DbContext FromRequestType<TRequest>()
            => (DbContext)s_serviceFactory(typeof(DbContext));
    }
}
