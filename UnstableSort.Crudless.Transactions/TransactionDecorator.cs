using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Transactions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class NoTransactionAttribute : Attribute
    {
    }

    public class TransactionDecoratorBase<TRequest, TResult>
    {
        public async Task<Response<TResult>> HandleAsync(TRequest request, Func<Task<Response<TResult>>> processRequest)
        {
            Response<TResult> response;

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                response = await processRequest();

                if (!response.HasErrors)
                    scope.Complete();
            }

            return response;
        }
    }

    public class TransactionDecorator<TRequest> 
        : IRequestHandler<TRequest> 
        where TRequest : IRequest
    {
        private readonly Func<IRequestHandler<TRequest>> _decorateeFactory;
        private readonly TransactionDecoratorBase<TRequest, NoResult> _transactionHandler;

        public TransactionDecorator(Func<IRequestHandler<TRequest>> decorateeFactory,
            TransactionDecoratorBase<TRequest, NoResult> transactionHandler)
        {
            _decorateeFactory = decorateeFactory;
            _transactionHandler = transactionHandler;
        }

        public Task<Response> HandleAsync(TRequest request, CancellationToken token)
        {
            return _transactionHandler
                .HandleAsync(request, () => _decorateeFactory().HandleAsync(request, token).ContinueWith(t => (Response<NoResult>)t.Result))
                .ContinueWith(t => (Response)t.Result);
        }
    }

    public class TransactionDecorator<TRequest, TResult> 
        : IRequestHandler<TRequest, TResult> 
        where TRequest : IRequest<TResult>
    {
        private readonly Func<IRequestHandler<TRequest, TResult>> _decorateeFactory;
        private readonly TransactionDecoratorBase<TRequest, TResult> _transactionHandler;

        public TransactionDecorator(Func<IRequestHandler<TRequest, TResult>> decorateeFactory,
            TransactionDecoratorBase<TRequest, TResult> transactionHandler)
        {
            _decorateeFactory = decorateeFactory;
            _transactionHandler = transactionHandler;
        }

        public Task<Response<TResult>> HandleAsync(TRequest request, CancellationToken token)
            => _transactionHandler.HandleAsync(request, () => _decorateeFactory().HandleAsync(request, token));
    }
}
