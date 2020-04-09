using System;
using System.Collections.Generic;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Validation;

namespace UnstableSort.Crudless.Requests
{
    [MaybeValidate]
    public class SynchronizeRequest<TEntity, TIn> 
        : InlineConfigurableRequest, ISynchronizeRequest<TEntity>
        where TEntity : class
    {
        public List<TIn> Items { get; set; } = new List<TIn>();

        public SynchronizeRequest()
        {
        }

        public SynchronizeRequest(List<TIn> items)
        {
            Items = items;
        }

        public void Configure(Action<InlineBulkRequestProfile<SynchronizeRequest<TEntity, TIn>, TIn>> configure)
        {
            var profile = new InlineBulkRequestProfile<SynchronizeRequest<TEntity, TIn>, TIn>(r => r.Items);

            configure(profile);

            Profile = profile;
        }
    }

    [MaybeValidate]
    public class SynchronizeRequest<TEntity, TIn, TOut> 
        : InlineConfigurableRequest, ISynchronizeRequest<TEntity, TOut>
        where TEntity : class
    {
        public List<TIn> Items { get; set; } = new List<TIn>();

        public SynchronizeRequest()
        {
        }

        public SynchronizeRequest(List<TIn> items)
        {
            Items = items;
        }

        public void Configure(Action<InlineBulkRequestProfile<SynchronizeRequest<TEntity, TIn, TOut>, TIn>> configure)
        {
            var profile = new InlineBulkRequestProfile<SynchronizeRequest<TEntity, TIn, TOut>, TIn>(r => r.Items);

            configure(profile);

            Profile = profile;
        }
    }

    [MaybeValidate]
    public class SynchronizeByIdRequest<TEntity, TIn> : SynchronizeRequest<TEntity, TIn>
        where TEntity : class
    {
        public SynchronizeByIdRequest(List<TIn> items) : base(items) { }
    }

    public class SynchronizeByIdRequestProfile<TEntity, TIn>
        : BulkRequestProfile<SynchronizeByIdRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public SynchronizeByIdRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Id");
        }
    }

    [MaybeValidate]
    public class SynchronizeByIdRequest<TEntity, TIn, TOut> : SynchronizeRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public SynchronizeByIdRequest(List<TIn> items) : base(items) { }
    }

    public class SynchronizeByIdRequestProfile<TEntity, TIn, TOut>
        : BulkRequestProfile<SynchronizeByIdRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public SynchronizeByIdRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Id");
        }
    }

    [MaybeValidate]
    public class SynchronizeByGuidRequest<TEntity, TIn> : SynchronizeRequest<TEntity, TIn>
        where TEntity : class
    {
        public SynchronizeByGuidRequest(List<TIn> items) : base(items) { }
    }

    public class SynchronizeByGuidRequestProfile<TEntity, TIn>
        : BulkRequestProfile<SynchronizeByGuidRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public SynchronizeByGuidRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Guid");
        }
    }

    [MaybeValidate]
    public class SynchronizeByGuidRequest<TEntity, TIn, TOut> : SynchronizeRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public SynchronizeByGuidRequest(List<TIn> items) : base(items) { }
    }

    public class SynchronizeByGuidRequestProfile<TEntity, TIn, TOut>
        : BulkRequestProfile<SynchronizeByGuidRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public SynchronizeByGuidRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Guid");
        }
    }

    [MaybeValidate]
    public class SynchronizeByNameRequest<TEntity, TIn> : SynchronizeRequest<TEntity, TIn>
        where TEntity : class
    {
        public SynchronizeByNameRequest(List<TIn> items) : base(items) { }
    }

    public class SynchronizeByNameRequestProfile<TEntity, TIn>
        : BulkRequestProfile<SynchronizeByNameRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public SynchronizeByNameRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Name");
        }
    }

    [MaybeValidate]
    public class SynchronizeByNameRequest<TEntity, TIn, TOut> : SynchronizeRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public SynchronizeByNameRequest(List<TIn> items) : base(items) { }
    }

    public class SynchronizeByNameRequestProfile<TEntity, TIn, TOut>
        : BulkRequestProfile<SynchronizeByNameRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public SynchronizeByNameRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Name");
        }
    }
}
