using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnstableSort.Crudless.Configuration;

namespace UnstableSort.Crudless.Requests
{
    public class InlineConfiguredRequest<TRequest>
        : IInlineConfiguredRequest<TRequest>
        where TRequest : ICrudlessRequest
    {
        public Action<InlineRequestProfile<TRequest>> Configure { get; set; }

        public InlineRequestProfile<TRequest> BuildProfile()
        {
            if (Configure == null)
                return null;

            var profile = new InlineRequestProfile<TRequest>();

            Configure(profile);

            return profile;
        }
    }

    public class InlineConfiguredBulkRequest<TRequest, TItem>
        : IInlineConfiguredBulkRequest<TRequest, TItem>
        where TRequest : ICrudlessRequest, IBulkRequest
    {
        public Action<InlineBulkRequestProfile<TRequest, TItem>> Configure { get; set; }

        protected Expression<Func<TRequest, IEnumerable<TItem>>> ItemSource { get; set; }

        public InlineBulkRequestProfile<TRequest, TItem> BuildProfile()
        {
            if (Configure == null)
                return null;

            var profile = ItemSource != null
                ? new InlineBulkRequestProfile<TRequest, TItem>(ItemSource)
                : new InlineBulkRequestProfile<TRequest, TItem>();

            Configure(profile);

            return profile;
        }
    }
}
