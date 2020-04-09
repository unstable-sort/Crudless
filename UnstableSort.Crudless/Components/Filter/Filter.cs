using System;
using System.Linq;
using IServiceProvider = UnstableSort.Crudless.Common.ServiceProvider.IServiceProvider;

namespace UnstableSort.Crudless
{
    public interface IFilter
    {
    }

    public abstract class Filter<TRequest, TEntity> : IFilter
        where TEntity : class
    {
        public abstract IQueryable<TEntity> Apply(TRequest request, IQueryable<TEntity> queryable);
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
            Filter<TRequest, TEntity> filter)
            where TEntity : class
        {
            return new InstanceFilterFactory(
                filter,
                new FunctionFilter((request, queryable) => filter.Apply((TRequest)request, (IQueryable<TEntity>)queryable)));
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
            where TFilter : Filter<TRequest, TEntity>
            where TEntity : class
        {
            return new TypeFilterFactory(
                provider =>
                {
                    var instance = (Filter<TRequest, TEntity>)provider.ProvideInstance(typeof(TFilter));
                    return new FunctionFilter((request, queryable)
                        => instance.Apply((TRequest)request, (IQueryable<TEntity>)queryable));
                });
        }

        public IBoxedFilter Create(IServiceProvider provider) => _filterFactory(provider);
    }
}
