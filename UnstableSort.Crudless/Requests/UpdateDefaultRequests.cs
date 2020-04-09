using System;
using AutoMapper;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Validation;

namespace UnstableSort.Crudless.Requests
{
    [MaybeValidate]
    public class UpdateRequest<TEntity, TIn> 
        : InlineConfigurableRequest, IUpdateRequest<TEntity>
        where TEntity : class
    {
        public TIn Item { get; set; }

        public UpdateRequest(TIn item) { Item = item; }

        public void Configure(Action<InlineRequestProfile<UpdateRequest<TEntity, TIn>>> configure)
        {
            var profile = new InlineRequestProfile<UpdateRequest<TEntity, TIn>>();

            configure(profile);

            Profile = profile;
        }
    }

    public class UpdateRequestProfile<TEntity, TIn>
        : RequestProfile<UpdateRequest<TEntity, TIn>>
        where TEntity : class
    {
        public UpdateRequestProfile()
        {
            ForEntity<TEntity>()
                .UpdateEntityWith((context, entity) =>
                {
                    return context.ServiceProvider
                        .ProvideInstance<IMapper>()
                        .Map(context.Request.Item, entity);
                });
        }
    }

    [MaybeValidate]
    public class UpdateRequest<TEntity, TIn, TOut> 
        : InlineConfigurableRequest, IUpdateRequest<TEntity, TOut>
        where TEntity : class
    {
        public TIn Item { get; set; }

        public UpdateRequest(TIn item) { Item = item; }

        public void Configure(Action<InlineRequestProfile<UpdateRequest<TEntity, TIn, TOut>>> configure)
        {
            var profile = new InlineRequestProfile<UpdateRequest<TEntity, TIn, TOut>>();

            configure(profile);

            Profile = profile;
        }
    }

    public class UpdateRequestProfile<TEntity, TIn, TOut>
        : RequestProfile<UpdateRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public UpdateRequestProfile()
        {
            ForEntity<TEntity>()
                .UpdateEntityWith((context, entity) =>
                {
                    return context.ServiceProvider
                        .ProvideInstance<IMapper>()
                        .Map(context.Request.Item, entity);
                });
        }
    }

    [MaybeValidate]
    public class UpdateRequest<TEntity, TKey, TIn, TOut>
        : InlineConfigurableRequest, IUpdateRequest<TEntity, TOut>
        where TEntity : class
    {
        public TKey Key { get; set; }

        public TIn Item { get; set; }

        public UpdateRequest(TKey key, TIn item)
        {
            Key = key;
            Item = item;
        }

        public void Configure(Action<InlineRequestProfile<UpdateRequest<TEntity, TKey, TIn, TOut>>> configure)
        {
            var profile = new InlineRequestProfile<UpdateRequest<TEntity, TKey, TIn, TOut>>();

            configure(profile);

            Profile = profile;
        }
    }

    public class UpdateRequestProfile<TEntity, TKey, TIn, TOut>
        : RequestProfile<UpdateRequest<TEntity, TKey, TIn, TOut>>
        where TEntity : class
    {
        public UpdateRequestProfile()
        {
            ForEntity<TEntity>()
                .UpdateEntityWith((context, entity) =>
                {
                    return context.ServiceProvider
                        .ProvideInstance<IMapper>()
                        .Map(context.Request.Item, entity);
                });
        }
    }

    [MaybeValidate]
    public class UpdateByIdRequest<TEntity, TIn, TOut> : UpdateRequest<TEntity, int, TIn, TOut>
        where TEntity : class
    {
        public UpdateByIdRequest(int id, TIn data) : base(id, data) { }
    }

    public class UpdateByIdRequestProfile<TEntity, TIn, TOut>
        : RequestProfile<UpdateByIdRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public UpdateByIdRequestProfile()
        {
            ForEntity<TEntity>().SelectBy(request => request.Key, "Id");
        }
    }

    [MaybeValidate]
    public class UpdateByGuidRequest<TEntity, TIn, TOut> : UpdateRequest<TEntity, Guid, TIn, TOut>
        where TEntity : class
    {
        public UpdateByGuidRequest(Guid guid, TIn data) : base(guid, data) { }
    }

    public class UpdateByGuidRequestProfile<TEntity, TIn, TOut>
        : RequestProfile<UpdateByGuidRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public UpdateByGuidRequestProfile()
        {
            ForEntity<TEntity>().SelectBy(request => request.Key, "Guid");
        }
    }

    [MaybeValidate]
    public class UpdateByNameRequest<TEntity, TIn, TOut> : UpdateRequest<TEntity, string, TIn, TOut>
        where TEntity : class
    {
        public UpdateByNameRequest(string name, TIn data) : base(name, data) { }
    }

    public class UpdateByNameRequestProfile<TEntity, TIn, TOut>
        : RequestProfile<UpdateByNameRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public UpdateByNameRequestProfile()
        {
            ForEntity<TEntity>().SelectBy(request => request.Key, "Name");
        }
    }
}
