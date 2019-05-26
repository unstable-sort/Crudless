using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UnstableSort.Crudless.Mediator.Hangfire
{
    public class BackgroundJobExecutor<TRequest, TResult> 
        where TRequest : IRequest<TResult>
    {
        private readonly IMediator _mediator;

        public BackgroundJobExecutor(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Execute(TRequest command)
        {
            var result = await _mediator.HandleAsync(command);

            if (result.HasErrors)
                throw new Exception(JsonConvert.SerializeObject(result.Errors));
        }
    }
}
