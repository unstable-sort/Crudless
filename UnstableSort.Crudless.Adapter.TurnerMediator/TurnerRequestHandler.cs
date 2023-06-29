using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnstableSort.Crudless.Adapter.TurnerMediator
{
    public class TurnerRequestHandler<TRequest> : Mediator.IRequestHandler<TurnerRequest<TRequest>>
        where TRequest : Turner.Infrastructure.Mediator.IRequest
    {
        private readonly Turner.Infrastructure.Mediator.IRequestHandler<TRequest> _turnerHandler;

        public TurnerRequestHandler(Turner.Infrastructure.Mediator.IRequestHandler<TRequest> turnerHandler)
        {
            _turnerHandler = turnerHandler;
        }

        public async Task<Mediator.Response> HandleAsync(TurnerRequest<TRequest> request, CancellationToken token)
        {
            var response = await _turnerHandler.HandleAsync(request.OriginalRequest);

            if (response.HasErrors)
            {
                return new Mediator.Response
                {
                    Errors = response.Errors
                        .Select(x => new Mediator.Error
                        {
                            PropertyName = x.PropertyName,
                            ErrorMessage = x.ErrorMessage
                        })
                        .ToList()
                };
            }

            return new Mediator.Response
            {
                Errors = new List<Mediator.Error>()
            };
        }
    }

    public class TurnerRequestHandler<TRequest, TResult> : Mediator.IRequestHandler<TurnerRequest<TRequest, TResult>, TResult>
        where TRequest : Turner.Infrastructure.Mediator.IRequest<TResult>
    {
        private readonly Turner.Infrastructure.Mediator.IRequestHandler<TRequest, TResult> _turnerHandler;

        public TurnerRequestHandler(Turner.Infrastructure.Mediator.IRequestHandler<TRequest, TResult> turnerHandler)
        {
            _turnerHandler = turnerHandler;
        }

        public async Task<Mediator.Response<TResult>> HandleAsync(TurnerRequest<TRequest, TResult> request, CancellationToken token)
        {
            var response = await _turnerHandler.HandleAsync(request.OriginalRequest);

            if (response.HasErrors)
            {
                return new Mediator.Response<TResult>
                {
                    Result = response.Data,
                    Errors = response.Errors
                        .Select(x => new Mediator.Error
                        {
                            PropertyName = x.PropertyName,
                            ErrorMessage = x.ErrorMessage
                        })
                        .ToList(),
                };
            }

            return new Mediator.Response<TResult>
            {
                Result = response.Data,
                Errors = new List<Mediator.Error>()
            };
        }
    }
}
