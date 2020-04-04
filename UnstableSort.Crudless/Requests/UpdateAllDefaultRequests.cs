using System.Collections.Generic;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Validation;

namespace UnstableSort.Crudless.Requests
{
    [MaybeValidate]
    public class UpdateAllRequest<TEntity, TIn> 
        : InlineConfiguredBulkRequest<UpdateAllRequest<TEntity, TIn>, TIn>,
          IUpdateAllRequest<TEntity>
        where TEntity : class
    {
        public List<TIn> Items { get; set; }

        public UpdateAllRequest(List<TIn> items)
        {
            Items = items;
            ItemSource = request => request.Items;
        }
    }

    [MaybeValidate]
    public class UpdateAllRequest<TEntity, TIn, TOut> 
        : InlineConfiguredBulkRequest<UpdateAllRequest<TEntity, TIn, TOut>, TIn>,
          IUpdateAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public List<TIn> Items { get; set; }

        public UpdateAllRequest(List<TIn> items)
        {
            Items = items;
            ItemSource = request => request.Items;
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
            Entity<TEntity>()
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
            Entity<TEntity>()
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
            Entity<TEntity>()
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
            Entity<TEntity>()
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
            Entity<TEntity>()
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
            Entity<TEntity>()
                .UseKeys("Name");
        }
    }
}
