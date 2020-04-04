using System;
using System.Linq;
using IServiceProvider = UnstableSort.Crudless.Common.ServiceProvider.IServiceProvider;

namespace UnstableSort.Crudless
{
    public enum SortDirection
    {
        Ascending = 0,
        Descending = 1,

        Default = Ascending
    }

    public interface ISorter<in TRequest, TEntity>
        where TEntity : class
    {
        IOrderedQueryable<TEntity> Sort(TRequest request, IQueryable<TEntity> queryable);
    }
    
    public interface IBoxedSorter
    {
        IOrderedQueryable Sort(object request, IQueryable queryable);
    }

    public interface ISorterFactory
    {
        IBoxedSorter Create(IServiceProvider provider);
    }

    public class FunctionSorter
        : IBoxedSorter
    {
        private readonly Func<object, IQueryable, IOrderedQueryable> _sortFunc;

        public FunctionSorter(Func<object, IQueryable, IOrderedQueryable> sortFunc)
        {
            _sortFunc = sortFunc;
        }

        public IOrderedQueryable Sort(object request, IQueryable queryable) => _sortFunc(request, queryable);
    }

    public class FunctionSorterFactory : ISorterFactory
    {
        private readonly IBoxedSorter _sorter;

        private FunctionSorterFactory(Func<object, IQueryable, IOrderedQueryable> sortFunc)
        {
            _sorter = new FunctionSorter(sortFunc);
        }

        internal static FunctionSorterFactory From<TRequest, TEntity>(
            Func<TRequest, IQueryable<TEntity>, IOrderedQueryable<TEntity>> sorter)
            where TEntity : class
        {
            return new FunctionSorterFactory(
                (request, queryable) => sorter((TRequest)request, (IQueryable<TEntity>)queryable));
        }

        public IBoxedSorter Create(IServiceProvider provider) => _sorter;
    }

    public class InstanceSorterFactory : ISorterFactory
    {
        private readonly object _instance;
        private IBoxedSorter _adaptedInstance;

        private InstanceSorterFactory(object instance, IBoxedSorter adaptedInstance)
        {
            _instance = instance;
            _adaptedInstance = adaptedInstance;
        }

        internal static InstanceSorterFactory From<TRequest, TEntity>(
            ISorter<TRequest, TEntity> sorter)
            where TEntity : class
        {
            return new InstanceSorterFactory(
                sorter,
                new FunctionSorter((request, queryable) => sorter.Sort((TRequest)request, (IQueryable<TEntity>)queryable)));
        }

        public IBoxedSorter Create(IServiceProvider provider) => _adaptedInstance;
    }

    public class TypeSorterFactory : ISorterFactory
    {
        private Func<IServiceProvider, IBoxedSorter> _sorterFactory;

        public TypeSorterFactory(Func<IServiceProvider, IBoxedSorter> sorterFactory)
        {
            _sorterFactory = sorterFactory;
        }
        
        internal static TypeSorterFactory From<TSorter, TRequest, TEntity>()
            where TSorter : ISorter<TRequest, TEntity>
            where TEntity : class
        {
            return new TypeSorterFactory(
                provider =>
                {
                    var instance = (ISorter<TRequest, TEntity>)provider.ProvideInstance(typeof(TSorter));
                    return new FunctionSorter((request, queryable)
                        => instance.Sort((TRequest)request, (IQueryable<TEntity>)queryable));
                });
        }

        public IBoxedSorter Create(IServiceProvider provider) => _sorterFactory(provider);
    }
}
