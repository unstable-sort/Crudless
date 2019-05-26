using System;
using System.Linq.Expressions;

namespace UnstableSort.Crudless.Configuration.Builders.Sort
{
    public class SortOperationBuilder<TEntity, TProp>
        where TEntity : class
    {
        public Expression<Func<TEntity, TProp>> Clause { get; set; }

        public SortDirection Direction { get; set; } = SortDirection.Default;
    }
}
