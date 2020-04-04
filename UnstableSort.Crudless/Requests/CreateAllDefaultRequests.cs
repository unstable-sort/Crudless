using System.Collections.Generic;
using AutoMapper;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Validation;

namespace UnstableSort.Crudless.Requests
{
    [MaybeValidate]
    public class CreateAllRequest<TEntity, TIn> 
        : InlineConfiguredBulkRequest<CreateAllRequest<TEntity, TIn>, TIn>,
          ICreateAllRequest<TEntity>
        where TEntity : class
    {
        public List<TIn> Items { get; set; } = new List<TIn>();

        public CreateAllRequest()
        {
            ItemSource = request => request.Items;
        }

        public CreateAllRequest(List<TIn> items)
        {
            Items = items;
            ItemSource = request => request.Items;
        }
    }

    public class CreateAllRequestProfile<TEntity, TIn>
        : BulkRequestProfile<CreateAllRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public CreateAllRequestProfile()
            : base(request => request.Items)
        {
            Entity<TEntity>()
                .CreateEntityWith((context, item) =>
                {
                    return context.ServiceProvider
                        .ProvideInstance<IMapper>()
                        .Map<TEntity>(item);
                });
        }
    }

    [MaybeValidate]
    public class CreateAllRequest<TEntity, TIn, TOut> 
        : InlineConfiguredBulkRequest<CreateAllRequest<TEntity, TIn, TOut>, TIn>,
          ICreateAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public List<TIn> Items { get; set; } = new List<TIn>();

        public CreateAllRequest()
        {
            ItemSource = request => request.Items;
        }

        public CreateAllRequest(List<TIn> items)
        {
            Items = items;
            ItemSource = request => request.Items;
        }
    }

    public class CreateAllRequestProfile<TEntity, TIn, TOut>
        : BulkRequestProfile<CreateAllRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public CreateAllRequestProfile()
            : base(request => request.Items)
        {
            Entity<TEntity>()
                .CreateEntityWith((context, item) =>
                {
                    return context.ServiceProvider
                        .ProvideInstance<IMapper>()
                        .Map<TEntity>(item);
                });
        }
    }
}
