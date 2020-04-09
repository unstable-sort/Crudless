using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Validation;

namespace UnstableSort.Crudless.Requests
{
    [MaybeValidate]
    public class CreateAllRequest<TEntity, TIn> 
        : InlineConfigurableRequest, ICreateAllRequest<TEntity>
        where TEntity : class
    {
        public List<TIn> Items { get; set; } = new List<TIn>();

        public CreateAllRequest()
        {
        }

        public CreateAllRequest(List<TIn> items)
        {
            Items = items;
        }

        public void Configure(Action<InlineBulkRequestProfile<CreateAllRequest<TEntity, TIn>, TIn>> configure)
        {
            var profile = new InlineBulkRequestProfile<CreateAllRequest<TEntity, TIn>, TIn>(r => r.Items);
            
            configure(profile);

            Profile = profile;
        }
    }

    public class CreateAllRequestProfile<TEntity, TIn>
        : BulkRequestProfile<CreateAllRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public CreateAllRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
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
        : InlineConfigurableRequest, ICreateAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public List<TIn> Items { get; set; } = new List<TIn>();

        public CreateAllRequest()
        {
        }

        public CreateAllRequest(List<TIn> items)
        {
            Items = items;
        }

        public void Configure(Action<InlineBulkRequestProfile<CreateAllRequest<TEntity, TIn, TOut>, TIn>> configure)
        {
            var profile = new InlineBulkRequestProfile<CreateAllRequest<TEntity, TIn, TOut>, TIn>(r => r.Items);

            configure(profile);

            Profile = profile;
        }
    }

    public class CreateAllRequestProfile<TEntity, TIn, TOut>
        : BulkRequestProfile<CreateAllRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public CreateAllRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .CreateEntityWith((context, item) =>
                {
                    return context.ServiceProvider
                        .ProvideInstance<IMapper>()
                        .Map<TEntity>(item);
                });
        }
    }
}
