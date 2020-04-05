using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnstableSort.Crudless.Adapter.TurnerMediator
{
    public class CrudlessRequestHandler<TRequest>
        : Turner.Infrastructure.Mediator.IRequestHandler<CrudlessRequest<TRequest>>
        where TRequest : Mediator.IRequest
    {
        private readonly Mediator.IRequestHandler<TRequest> _crudlessHandler;

        public CrudlessRequestHandler(Mediator.IRequestHandler<TRequest> crudlessHandler)
        {
            _crudlessHandler = crudlessHandler;
        }

        public async Task<Turner.Infrastructure.Mediator.Response> HandleAsync(CrudlessRequest<TRequest> request)
        {
            var response = await _crudlessHandler.HandleAsync(request.OriginalRequest, default);
            
            if (response.HasErrors)
            {
                return new Turner.Infrastructure.Mediator.Response
                {
                    Errors = response.Errors
                        .Select(x => new Turner.Infrastructure.Mediator.Error
                        {
                            PropertyName = x.PropertyName,
                            ErrorMessage = x.ErrorMessage
                        })
                        .ToList()
                };
            }

            return new Turner.Infrastructure.Mediator.Response
            {
                Errors = new List<Turner.Infrastructure.Mediator.Error>()
            };
        }
    }

    public class CrudlessRequestHandler<TRequest, TResult>
        : Turner.Infrastructure.Mediator.IRequestHandler<CrudlessRequest<TRequest, TResult>, TResult>
        where TRequest : Mediator.IRequest<TResult>
    {
        private readonly Mediator.IRequestHandler<TRequest, TResult> _crudlessHandler;

        public CrudlessRequestHandler(Mediator.IRequestHandler<TRequest, TResult> crudlessHandler)
        {
            _crudlessHandler = crudlessHandler;
        }

        public async Task<Turner.Infrastructure.Mediator.Response<TResult>> HandleAsync(CrudlessRequest<TRequest, TResult> request)
        {
            var response = await _crudlessHandler.HandleAsync(request.OriginalRequest, default);
            
            if (response.HasErrors)
            {
                return new Turner.Infrastructure.Mediator.Response<TResult>
                {
                    Errors = response.Errors
                        .Select(x => new Turner.Infrastructure.Mediator.Error
                        {
                            PropertyName = x.PropertyName,
                            ErrorMessage = x.ErrorMessage
                        })
                        .ToList()
                };
            }

            return new Turner.Infrastructure.Mediator.Response<TResult>
            {
                Errors = new List<Turner.Infrastructure.Mediator.Error>(),
                Data = response.Result
            };
        }
    }
}
