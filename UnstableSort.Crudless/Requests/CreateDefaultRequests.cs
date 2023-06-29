using System;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Validation;

namespace UnstableSort.Crudless.Requests
{
    [MaybeValidate]
    public class CreateRequest<TEntity, TIn> 
        : InlineConfigurableRequest, ICreateRequest<TEntity>
        where TEntity : class
    {
        public TIn Item { get; set; }

        public CreateRequest() { }

        public CreateRequest(TIn item) { Item = item; }

        public void Configure(Action<InlineRequestProfile<CreateRequest<TEntity, TIn>>> configure)
        {
            var profile = new InlineRequestProfile<CreateRequest<TEntity, TIn>>();

            configure(profile);

            Profile = profile;
        }
    }

    public class CreateRequestProfile<TEntity, TIn>
        : RequestProfile<CreateRequest<TEntity, TIn>>
        where TEntity : class
    {
        public CreateRequestProfile()
        {
            ForEntity<TEntity>()
                .CreateEntityWith(context =>
                {
                    return context.ServiceProvider
                        .ProvideInstance<IObjectMapper>()
                        .Map<TIn, TEntity>(context.Request.Item);
                });
        }
    }

    [MaybeValidate]
    public class CreateRequest<TEntity, TIn, TOut> 
        : InlineConfigurableRequest, ICreateRequest<TEntity, TOut>
        where TEntity : class
    {
        public TIn Item { get; set; }

        public CreateRequest() { }

        public CreateRequest(TIn item) { Item = item; }

        public void Configure(Action<InlineRequestProfile<CreateRequest<TEntity, TIn, TOut>>> configure)
        {
            var profile = new InlineRequestProfile<CreateRequest<TEntity, TIn, TOut>>();

            configure(profile);

            Profile = profile;
        }
    }

    public class CreateRequestProfile<TEntity, TIn, TOut>
        : RequestProfile<CreateRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public CreateRequestProfile()
        {
            ForEntity<TEntity>()
                .CreateEntityWith(context =>
                {
                    return context.ServiceProvider
                        .ProvideInstance<IObjectMapper>()
                        .Map<TIn, TEntity>(context.Request.Item);
                });
        }
    }
}
