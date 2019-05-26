using System.Threading;
using System.Threading.Tasks;

namespace UnstableSort.Crudless.Mediator.Tests.Fakes
{
    public class RequestWithoutResponse : IRequest
    {
    }

    public class RequestWithoutResponseHandler : IRequestHandler<RequestWithoutResponse>
    {
        public Task<Response> HandleAsync(RequestWithoutResponse request, CancellationToken token)
        {
            return Response.SuccessAsync();
        }
    }
    
    public class RequestWithResponse : IRequest<string>
    {
        public string Foo { get; set; } = "Bar";
    }

    public class RequestWithResponseHandler : IRequestHandler<RequestWithResponse, string>
    {
        public Task<Response<string>> HandleAsync(RequestWithResponse request, CancellationToken token)
        {
            return request.Foo.AsResponseAsync();
        }
    }
}
