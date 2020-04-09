using System;
using UnstableSort.Crudless.Configuration;
using UnstableSort.Crudless.Validation;

namespace UnstableSort.Crudless.Requests
{
    [MaybeValidate]
    public class GetAllRequest<TEntity, TOut>
        : InlineConfigurableRequest, IGetAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public void Configure(Action<InlineRequestProfile<GetAllRequest<TEntity, TOut>>> configure)
        {
            var profile = new InlineRequestProfile<GetAllRequest<TEntity, TOut>>();

            configure(profile);

            Profile = profile;
        }
    }
}
