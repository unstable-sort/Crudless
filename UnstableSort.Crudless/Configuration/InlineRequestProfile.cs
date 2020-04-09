using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnstableSort.Crudless.Requests;

namespace UnstableSort.Crudless.Configuration
{
    public class InlineRequestProfile<TRequest> 
        : RequestProfile<TRequest>
        where TRequest : ICrudlessRequest
    {
    }

    public class InlineBulkRequestProfile<TRequest, TItem> 
        : BulkRequestProfile<TRequest, TItem>
        where TRequest : ICrudlessRequest, IBulkRequest
    {
        public InlineBulkRequestProfile() 
            : base()
        {
        }

        public InlineBulkRequestProfile(Expression<Func<TRequest, ICollection<TItem>>> defaultItemSource) 
            : base(defaultItemSource)
        {
        }
    }

    public interface IInlineConfiguredRequest
    {
        object BuildProfile();
    }

    public interface IInlineConfiguredBulkRequest
    {
        object BuildProfile();
    }
}
