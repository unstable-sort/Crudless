using System.Linq;

namespace UnstableSort.Crudless
{
    public interface IObjectMapper
    {
        TDestination Map<TSource, TDestination>(TSource source);

        TDestination Map<TDestination>(object source);

        TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

        TSource Clone<TSource>(TSource source);

        IQueryable<TDestination> Project<TSource, TDestination>(IQueryable<TSource> source);
    }
}
