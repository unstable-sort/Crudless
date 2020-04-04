using Turner.Infrastructure.Mediator.Decorators;
using UnstableSort.Crudless.Transactions;

namespace UnstableSort.Crudless.Adapter.TurnerMediator
{
    [NoTransaction, DoNotValidate]
    public class CrudlessRequest<TRequest> : Turner.Infrastructure.Mediator.IRequest
        where TRequest : Mediator.IRequest
    {
        public CrudlessRequest(TRequest request)
            => Request = request;

        public TRequest Request { get; }
    }

    [NoTransaction, DoNotValidate]
    public class CrudlessRequest<TRequest, TResult> : Turner.Infrastructure.Mediator.IRequest<TResult>
        where TRequest : Mediator.IRequest<TResult>
    {
        public CrudlessRequest(TRequest request)
            => Request = request;

        public TRequest Request { get; }
    }
}
