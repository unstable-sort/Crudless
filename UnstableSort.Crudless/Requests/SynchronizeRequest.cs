using System.Collections.Generic;
using System.Linq;
using UnstableSort.Crudless.Mediator;
// ReSharper disable UnusedTypeParameter

namespace UnstableSort.Crudless.Requests
{
    public interface ISynchronizeRequest : IBulkRequest
    {
    }

    public interface ISynchronizeRequest<TEntity> : ISynchronizeRequest, IRequest
        where TEntity : class
    {
    }

    public interface ISynchronizeRequest<TEntity, TOut> : ISynchronizeRequest, IRequest<SynchronizeResult<TOut>>
        where TEntity : class
    {
    }

    public class SynchronizeResult<TOut> : IResultCollection<TOut>
    {
        public List<TOut> Items { get; set; }

        public SynchronizeResult(IEnumerable<TOut> items)
        {
            Items = items.ToList();
        }
    }
}
