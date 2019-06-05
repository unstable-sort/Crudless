using System.Collections.Generic;
using System.Linq;
using UnstableSort.Crudless.Mediator;
// ReSharper disable UnusedTypeParameter

namespace UnstableSort.Crudless.Requests
{
    public interface IDeleteAllRequest
    {
    }

    public interface IDeleteAllRequest<TEntity> 
        : IDeleteAllRequest, IRequest, IAuditedRequest<TEntity>, ICrudlessRequest<TEntity>
        where TEntity : class
    {
    }

    public interface IDeleteAllRequest<TEntity, TOut> 
        : IDeleteAllRequest, IRequest<DeleteAllResult<TOut>>, IAuditedRequest<TEntity>, ICrudlessRequest<TEntity, TOut>
        where TEntity : class
    {
    }

    public class DeleteAllResult<TOut> : IResultCollection<TOut>
    {
        public List<TOut> Items { get; set; }

        public DeleteAllResult(IEnumerable<TOut> items)
        {
            Items = items.ToList();
        }
    }
}
