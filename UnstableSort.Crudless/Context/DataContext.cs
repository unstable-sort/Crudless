using UnstableSort.Crudless.Configuration;

namespace UnstableSort.Crudless.Context
{
    public class DataContext<TEntity>
        where TEntity : class
    {
        public EntitySet<TEntity> EntitySet { get; private set; }

        public IRequestConfig Configuration { get; }

        public DataContext(IRequestConfig config) : this(null, config) { }

        public DataContext(EntitySet<TEntity> entitySet, IRequestConfig config)
        {
            EntitySet = entitySet;
            Configuration = config;
        }

        internal DataContext<TEntity> WithEntitySet(EntitySet<TEntity> entitySet)
        {
            EntitySet = entitySet;

            return this;
        }
    }
}
