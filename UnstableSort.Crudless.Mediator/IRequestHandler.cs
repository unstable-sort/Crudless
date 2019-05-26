using System.Threading;
using System.Threading.Tasks;

namespace UnstableSort.Crudless.Mediator
{
    public interface IRequestHandler
    {
    }

    public interface IRequestHandler<in TRequest, TResult>
        : IRequestHandler
        where TRequest : IRequest<TResult>
    {
        Task<Response<TResult>> HandleAsync(TRequest request, CancellationToken token);
    }

    public interface IRequestHandler<in TRequest>
        : IRequestHandler
        where TRequest : IRequest<NoResult>
    {
        Task<Response> HandleAsync(TRequest request, CancellationToken token);
    }
}
