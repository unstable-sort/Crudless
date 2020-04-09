using System;
using System.Collections.Generic;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Validation;

namespace UnstableSort.Crudless.Requests
{
    [MaybeValidate]
    public class UpdateAllRequest<TEntity, TIn> 
        : InlineConfigurableRequest, IUpdateAllRequest<TEntity>
        where TEntity : class
    {
        public List<TIn> Items { get; set; }

        public UpdateAllRequest()
        {
        }

        public UpdateAllRequest(List<TIn> items)
        {
            Items = items;
        }

        public void Configure(Action<InlineBulkRequestProfile<UpdateAllRequest<TEntity, TIn>, TIn>> configure)
        {
            var profile = new InlineBulkRequestProfile<UpdateAllRequest<TEntity, TIn>, TIn>(r => r.Items);

            configure(profile);

            Profile = profile;
        }
    }

    [MaybeValidate]
    public class UpdateAllRequest<TEntity, TIn, TOut> 
        : InlineConfigurableRequest, IUpdateAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public List<TIn> Items { get; set; }

        public UpdateAllRequest()
        {
        }

        public UpdateAllRequest(List<TIn> items)
        {
            Items = items;
        }

        public void Configure(Action<InlineBulkRequestProfile<UpdateAllRequest<TEntity, TIn, TOut>, TIn>> configure)
        {
            var profile = new InlineBulkRequestProfile<UpdateAllRequest<TEntity, TIn, TOut>, TIn>(r => r.Items);

            configure(profile);

            Profile = profile;
        }
    }
    
    [MaybeValidate]
    public class UpdateAllByIdRequest<TEntity, TIn> : UpdateAllRequest<TEntity, TIn>
        where TEntity : class
    {
        public UpdateAllByIdRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByIdRequestProfile<TEntity, TIn>
        : BulkRequestProfile<UpdateAllByIdRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public UpdateAllByIdRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Id");
        }
    }

    [MaybeValidate]
    public class UpdateAllByIdRequest<TEntity, TIn, TOut> : UpdateAllRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public UpdateAllByIdRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByIdRequestProfile<TEntity, TIn, TOut>
        : BulkRequestProfile<UpdateAllByIdRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public UpdateAllByIdRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Id");
        }
    }

    [MaybeValidate]
    public class UpdateAllByGuidRequest<TEntity, TIn> : UpdateAllRequest<TEntity, TIn>
            where TEntity : class
    {
        public UpdateAllByGuidRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByGuidRequestProfile<TEntity, TIn>
        : BulkRequestProfile<UpdateAllByGuidRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public UpdateAllByGuidRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Guid");
        }
    }
    
    [MaybeValidate]
    public class UpdateAllByGuidRequest<TEntity, TIn, TOut> : UpdateAllRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public UpdateAllByGuidRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByGuidRequestProfile<TEntity, TIn, TOut>
        : BulkRequestProfile<UpdateAllByGuidRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public UpdateAllByGuidRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Guid");
        }
    }

    [MaybeValidate]
    public class UpdateAllByNameRequest<TEntity, TIn> : UpdateAllRequest<TEntity, TIn>
            where TEntity : class
    {
        public UpdateAllByNameRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByNameRequestProfile<TEntity, TIn>
        : BulkRequestProfile<UpdateAllByNameRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public UpdateAllByNameRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Name");
        }
    }

    [MaybeValidate]
    public class UpdateAllByNameRequest<TEntity, TIn, TOut> : UpdateAllRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public UpdateAllByNameRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByNameRequestProfile<TEntity, TIn, TOut>
        : BulkRequestProfile<UpdateAllByNameRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public UpdateAllByNameRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Name");
        }
    }
}
