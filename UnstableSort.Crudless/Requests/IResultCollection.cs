using System.Collections.Generic;

namespace UnstableSort.Crudless.Requests
{
    public interface IResultCollection<TOut>
    {
        List<TOut> Items { get; set; }
    }
}
