using UnstableSort.Crudless.Validation;

namespace UnstableSort.Crudless.Requests
{
    [MaybeValidate]
    public class GetAllRequest<TEntity, TOut>
        : InlineConfiguredRequest<GetAllRequest<TEntity, TOut>>,
          IGetAllRequest<TEntity, TOut>
        where TEntity : class
    {
    }
}
