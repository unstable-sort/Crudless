using System;
using System.Linq;
using IServiceProvider = UnstableSort.Crudless.Common.ServiceProvider.IServiceProvider;

namespace UnstableSort.Crudless
{
    public interface IFilter<in TRequest, TEntity>
        where TEntity : class
    {
        IQueryable<TEntity> Filter(TRequest request, IQueryable<TEntity> queryable);
    }

    public interface IBoxedFilter
    {
        IQueryable Filter(object request, IQueryable queryable);
    }

    public interface IFilterFactory
    {
        IBoxedFilter Create(IServiceProvider provider);
    }

    public class FunctionFilter
        : IBoxedFilter
    {
        private readonly Func<object, IQueryable, IQueryable> _filterFunc;

        public FunctionFilter(Func<object, IQueryable, IQueryable> filterFunc)
        {
            _filterFunc = filterFunc;
        }

        public IQueryable Filter(object request, IQueryable queryable) => _filterFunc(request, queryable);
    }

    public class FunctionFilterFactory : IFilterFactory
    {
        private readonly IBoxedFilter _filter;

        private FunctionFilterFactory(Func<object, IQueryable, IQueryable> filterFunc)
        {
            _filter = new FunctionFilter(filterFunc);
        }

        internal static FunctionFilterFactory From<TRequest, TEntity>(
            Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>> filter)
            where TEntity : class
        {
            return new FunctionFilterFactory(
                (request, queryable) => filter((TRequest)request, (IQueryable<TEntity>)queryable));
        }
        
        public IBoxedFilter Create(IServiceProvider provider) => _filter;
    }

    public class InstanceFilterFactory : IFilterFactory
    {
        private readonly object _instance;
        private IBoxedFilter _adaptedInstance;

        private InstanceFilterFactory(object instance, IBoxedFilter adaptedInstance)
        {
            _instance = instance;
            _adaptedInstance = adaptedInstance;
        }

        internal static InstanceFilterFactory From<TRequest, TEntity>(
            IFilter<TRequest, TEntity> filter)
            where TEntity : class
        {
            return new InstanceFilterFactory(
                filter,
                new FunctionFilter((request, queryable) => filter.Filter((TRequest)request, (IQueryable<TEntity>)queryable)));
        }

        public IBoxedFilter Create(IServiceProvider provider) => _adaptedInstance;
    }

    public class TypeFilterFactory : IFilterFactory
    {
        private Func<IServiceProvider, IBoxedFilter> _filterFactory;

        public TypeFilterFactory(Func<IServiceProvider, IBoxedFilter> filterFactory)
        {
            _filterFactory = filterFactory;
        }
        
        internal static TypeFilterFactory From<TFilter, TRequest, TEntity>()
            where TFilter : IFilter<TRequest, TEntity>
            where TEntity : class
        {
            return new TypeFilterFactory(
                provider =>
                {
                    var instance = (IFilter<TRequest, TEntity>)provider.ProvideInstance(typeof(TFilter));
                    return new FunctionFilter((request, queryable)
                        => instance.Filter((TRequest)request, (IQueryable<TEntity>)queryable));
                });
        }

        public IBoxedFilter Create(IServiceProvider provider) => _filterFactory(provider);
    }
}
