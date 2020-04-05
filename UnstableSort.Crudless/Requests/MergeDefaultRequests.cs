using System.Collections.Generic;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Validation;

namespace UnstableSort.Crudless.Requests
{
    [MaybeValidate]
    public class MergeRequest<TEntity, TIn> 
        : InlineConfiguredBulkRequest<MergeRequest<TEntity, TIn>, TIn>,
          IMergeRequest<TEntity>
        where TEntity : class
    {
        public List<TIn> Items { get; set; } = new List<TIn>();

        public MergeRequest()
        {
            ItemSource = request => request.Items;
        }

        public MergeRequest(List<TIn> items)
        {
            Items = items;
            ItemSource = request => request.Items;
        }
    }
    
    [MaybeValidate]
    public class MergeRequest<TEntity, TIn, TOut> 
        : InlineConfiguredBulkRequest<MergeRequest<TEntity, TIn, TOut>, TIn>,
          IMergeRequest<TEntity, TOut>
        where TEntity : class
    {
        public List<TIn> Items { get; set; } = new List<TIn>();

        public MergeRequest()
        {
            ItemSource = request => request.Items;
        }

        public MergeRequest(List<TIn> items)
        {
            Items = items;
            ItemSource = request => request.Items;
        }
    }

    [MaybeValidate]
    public class MergeByIdRequest<TEntity, TIn> : MergeRequest<TEntity, TIn>
        where TEntity : class
    {
        public MergeByIdRequest(List<TIn> items) : base(items) { }
    }

    public class MergeByIdRequestProfile<TEntity, TIn>
        : BulkRequestProfile<MergeByIdRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public MergeByIdRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Id");
        }
    }

    [MaybeValidate]
    public class MergeByIdRequest<TEntity, TIn, TOut> : MergeRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public MergeByIdRequest(List<TIn> items) : base(items) { }
    }

    public class MergeByIdRequestProfile<TEntity, TIn, TOut>
        : BulkRequestProfile<MergeByIdRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public MergeByIdRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Id");
        }
    }

    [MaybeValidate]
    public class MergeByGuidRequest<TEntity, TIn> : MergeRequest<TEntity, TIn>
        where TEntity : class
    {
        public MergeByGuidRequest(List<TIn> items) : base(items) { }
    }

    public class MergeByGuidRequestProfile<TEntity, TIn>
        : BulkRequestProfile<MergeByGuidRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public MergeByGuidRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Guid");
        }
    }
    
    [MaybeValidate]
    public class MergeByGuidRequest<TEntity, TIn, TOut> : MergeRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public MergeByGuidRequest(List<TIn> items) : base(items) { }
    }

    public class MergeByGuidRequestProfile<TEntity, TIn, TOut>
        : BulkRequestProfile<MergeByGuidRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public MergeByGuidRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Guid");
        }
    }

    [MaybeValidate]
    public class MergeByNameRequest<TEntity, TIn> : MergeRequest<TEntity, TIn>
        where TEntity : class
    {
        public MergeByNameRequest(List<TIn> items) : base(items) { }
    }

    public class MergeByNameRequestProfile<TEntity, TIn>
        : BulkRequestProfile<MergeByNameRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public MergeByNameRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Name");
        }
    }

    [MaybeValidate]
    public class MergeByNameRequest<TEntity, TIn, TOut> : MergeRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public MergeByNameRequest(List<TIn> items) : base(items) { }
    }

    public class MergeByNameRequestProfile<TEntity, TIn, TOut>
        : BulkRequestProfile<MergeByNameRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public MergeByNameRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Name");
        }
    }
}
