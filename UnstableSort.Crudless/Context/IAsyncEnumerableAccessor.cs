using System.Collections.Generic;

namespace UnstableSort.Crudless.Context
{
    internal interface IAsyncEnumerableAccessor<out T>
    {
        IAsyncEnumerable<T> AsyncEnumerable { get; }
    }
}
