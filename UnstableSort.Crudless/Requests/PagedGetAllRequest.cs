﻿using System.Collections.Generic;
using System.Linq;
using UnstableSort.Crudless.Mediator;
// ReSharper disable UnusedTypeParameter

namespace UnstableSort.Crudless.Requests
{
    public interface IPagedGetAllRequest 
        : IPagedRequest, IGetAllRequest
    {
    }

    public interface IPagedGetAllRequest<TEntity, TOut> 
        : IPagedGetAllRequest, IRequest<PagedGetAllResult<TOut>>, ICrudlessRequest<TEntity, TOut>
        where TEntity : class
    {
    }

    public class PagedGetAllResult<TOut> : IResultCollection<TOut>
    {
        public List<TOut> Items { get; set; }

        public int PageNumber { get; }

        public int PageSize { get; }

        public int PageCount { get; }

        public int TotalItemCount { get; }

        public PagedGetAllResult(IEnumerable<TOut> items, int pageNumber, int pageSize, int pageCount, int totalCount)
        {
            Items = items.ToList();
            PageNumber = pageNumber;
            PageSize = pageSize;
            PageCount = pageCount;
            TotalItemCount = totalCount;
        }
    }
}
