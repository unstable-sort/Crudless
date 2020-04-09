using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Integration.EntityFrameworkCore.Transactions
{
    public class TransactionScopeTransactionDecorator<TRequest> 
        : IRequestHandler<TRequest> 
        where TRequest : IRequest
    {
        private readonly Func<IRequestHandler<TRequest>> _decorateeFactory;

        public TransactionScopeTransactionDecorator(Func<IRequestHandler<TRequest>> decorateeFactory)
        {
            _decorateeFactory = decorateeFactory;
        }

        public async Task<Response> HandleAsync(TRequest request, CancellationToken token)
        {
            Response response;

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                response = await _decorateeFactory().HandleAsync(request, token);

                if (!response.HasErrors)
                    scope.Complete();
            }

            return response;
        }
    }

    public class TransactionScopeTransactionDecorator<TRequest, TResult> 
        : IRequestHandler<TRequest, TResult> 
        where TRequest : IRequest<TResult>
    {
        private readonly Func<IRequestHandler<TRequest, TResult>> _decorateeFactory;

        public TransactionScopeTransactionDecorator(Func<IRequestHandler<TRequest, TResult>> decorateeFactory)
        {
            _decorateeFactory = decorateeFactory;
        }

        public async Task<Response<TResult>> HandleAsync(TRequest request, CancellationToken token)
        {
            Response<TResult> response;

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                response = await _decorateeFactory().HandleAsync(request, token);

                if (!response.HasErrors)
                    scope.Complete();
            }

            return response;
        }
    }
}
