using System.Collections.Generic;
using System.Linq;
using UnstableSort.Crudless.Mediator;
// ReSharper disable UnusedTypeParameter

namespace UnstableSort.Crudless.Requests
{
    public interface ICreateAllRequest 
        : IBulkRequest
    {
    }

    public interface ICreateAllRequest<TEntity> 
        : ICreateAllRequest, IRequest, IAuditedRequest<TEntity>, ICrudlessRequest<TEntity>
        where TEntity : class
    {
    }

    public interface ICreateAllRequest<TEntity, TOut> 
        : ICreateAllRequest, IRequest<CreateAllResult<TOut>>, IAuditedRequest<TEntity>, ICrudlessRequest<TEntity, TOut>
        where TEntity : class
    {
    }

    public class CreateAllResult<TOut> 
        : IResultCollection<TOut>
    {
        public List<TOut> Items { get; set; }

        public CreateAllResult(IEnumerable<TOut> items)
        {
            Items = items.ToList();
        }
    }
}
