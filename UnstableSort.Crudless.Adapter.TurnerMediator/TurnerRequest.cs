using Turner.Infrastructure.Mediator.Decorators;
using UnstableSort.Crudless.Integration.EntityFrameworkCore.Transactions;

namespace UnstableSort.Crudless.Adapter.TurnerMediator
{
    [NoTransaction, DoNotValidate]
    public class TurnerRequest<TRequest> : Mediator.IRequest
        where TRequest : Turner.Infrastructure.Mediator.IRequest
    {
        public TurnerRequest(TRequest request)
            => OriginalRequest = request;

        public TRequest OriginalRequest { get; }
    }

    [NoTransaction, DoNotValidate]
    public class TurnerRequest<TRequest, TResult> : Mediator.IRequest<TResult>
        where TRequest : Turner.Infrastructure.Mediator.IRequest<TResult>
    {
        public TurnerRequest(TRequest request)
            => OriginalRequest = request;

        public TRequest OriginalRequest { get; }
    }
}
