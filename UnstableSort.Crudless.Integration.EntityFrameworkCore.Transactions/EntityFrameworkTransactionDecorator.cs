using System;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Context;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Integration.EntityFrameworkCore.Transactions
{
    public class EntityFrameworkTransactionDecorator<TRequest> 
        : IRequestHandler<TRequest> 
        where TRequest : IRequest
    {
        private readonly IEntityContext _context;
        private readonly Func<IRequestHandler<TRequest>> _decorateeFactory;

        public EntityFrameworkTransactionDecorator(IEntityContext context, Func<IRequestHandler<TRequest>> decorateeFactory)
        {
            _context = context;
            _decorateeFactory = decorateeFactory;
        }

        public async Task<Response> HandleAsync(TRequest request, CancellationToken token)
        {
            if (_context.HasTransaction)
                return await _decorateeFactory().HandleAsync(request, token);
            
            using (var transaction = await _context.BeginTransactionAsync<TRequest>(token))
            {
                var response = await _decorateeFactory().HandleAsync(request, token);

                if (response.HasErrors)
                {
                    transaction.Rollback();
                    return response;
                }

                transaction.Commit();

                return response;
            }
        }
    }

    public class EntityFrameworkTransactionDecorator<TRequest, TResult> 
        : IRequestHandler<TRequest, TResult> 
        where TRequest : IRequest<TResult>
    {
        private readonly IEntityContext _context;
        private readonly Func<IRequestHandler<TRequest, TResult>> _decorateeFactory;

        public EntityFrameworkTransactionDecorator(IEntityContext context, Func<IRequestHandler<TRequest, TResult>> decorateeFactory)
        {
            _context = context;
            _decorateeFactory = decorateeFactory;
        }

        public async Task<Response<TResult>> HandleAsync(TRequest request, CancellationToken token)
        {
            if (_context.HasTransaction)
                return await _decorateeFactory().HandleAsync(request, token);

            using (var transaction = await _context.BeginTransactionAsync<TRequest>(token))
            {
                var response = await _decorateeFactory().HandleAsync(request, token);

                if (response.HasErrors)
                {
                    transaction.Rollback();
                    return response;
                }

                transaction.Commit();

                return response;
            }
        }
    }
}
