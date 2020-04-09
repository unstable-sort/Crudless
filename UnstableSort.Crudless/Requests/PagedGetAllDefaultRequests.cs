using System;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Validation;

namespace UnstableSort.Crudless.Requests
{
    [MaybeValidate]
    public class PagedGetAllRequest<TEntity, TOut> 
        : InlineConfigurableRequest, IPagedGetAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public void Configure(Action<InlineRequestProfile<PagedGetAllRequest<TEntity, TOut>>> configure)
        {
            var profile = new InlineRequestProfile<PagedGetAllRequest<TEntity, TOut>>();

            configure(profile);

            Profile = profile;
        }
    }
}
