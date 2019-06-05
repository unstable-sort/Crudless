using System.Collections.Generic;
using System.Linq;
using UnstableSort.Crudless.Mediator;
// ReSharper disable UnusedTypeParameter

namespace UnstableSort.Crudless.Requests
{
    public interface IGetAllRequest
    {
    }

    public interface IGetAllRequest<TEntity, TOut> 
        : IGetAllRequest, IRequest<GetAllResult<TOut>>, ICrudlessRequest<TEntity, TOut>
        where TEntity : class
    {
    }
    
    public class GetAllResult<TOut> : IResultCollection<TOut>
    {
        public List<TOut> Items { get; set; }

        public GetAllResult(IEnumerable<TOut> items)
        {
            Items = items.ToList();
        }
    }
}
