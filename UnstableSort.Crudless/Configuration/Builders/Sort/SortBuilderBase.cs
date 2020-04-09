using System.Diagnostics.CodeAnalysis;

namespace UnstableSort.Crudless.Configuration.Builders.Sort
{
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    public abstract class SortBuilderBase<TRequest, TEntity>
        where TEntity : class
    {
        internal abstract ISorterFactory Build();
    }
}
