using AutoMapper;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Validation;

namespace UnstableSort.Crudless.Requests
{
    [MaybeValidate]
    public class CreateRequest<TEntity, TIn> 
        : InlineConfiguredRequest<CreateRequest<TEntity, TIn>>,
          ICreateRequest<TEntity>
        where TEntity : class
    {
        public TIn Item { get; set; }

        public CreateRequest() { }

        public CreateRequest(TIn item) { Item = item; }
    }

    public class CreateRequestProfile<TEntity, TIn>
        : RequestProfile<CreateRequest<TEntity, TIn>>
        where TEntity : class
    {
        public CreateRequestProfile()
        {
            Entity<TEntity>()
                .CreateEntityWith(context =>
                {
                    return context.ServiceProvider
                        .ProvideInstance<IMapper>()
                        .Map<TEntity>(context.Request.Item);
                });
        }
    }

    [MaybeValidate]
    public class CreateRequest<TEntity, TIn, TOut> 
        : InlineConfiguredRequest<CreateRequest<TEntity, TIn, TOut>>,
          ICreateRequest<TEntity, TOut>
        where TEntity : class
    {
        public TIn Item { get; set; }

        public CreateRequest() { }

        public CreateRequest(TIn item) { Item = item; }
    }

    public class CreateRequestProfile<TEntity, TIn, TOut>
        : RequestProfile<CreateRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public CreateRequestProfile()
        {
            Entity<TEntity>()
                .CreateEntityWith(context =>
                {
                    return context.ServiceProvider
                        .ProvideInstance<IMapper>()
                        .Map<TEntity>(context.Request.Item);
                });
        }
    }
}
