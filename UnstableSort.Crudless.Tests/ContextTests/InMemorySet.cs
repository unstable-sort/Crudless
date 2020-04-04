using System.Collections.Generic;
using UnstableSort.Crudless.Context.Utilities;

namespace UnstableSort.Crudless.Tests.ContextTests
{
    public interface IInMemorySet
    {
    }

    public class InMemorySet<TEntity> : CollectionEntitySet<TEntity>, IInMemorySet
        where TEntity : class
    {
        public InMemorySet(List<TEntity> items)
            : base(items)
        {
            Items = items;
        }

        public List<TEntity> Items { get; }

        public int Id { get; set; } = 1;
    }
}
