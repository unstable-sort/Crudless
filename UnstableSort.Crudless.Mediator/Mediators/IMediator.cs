using System.Threading;
using System.Threading.Tasks;

namespace UnstableSort.Crudless.Mediator
{
    public interface IMediator
    {
        Task<Response<TResult>> HandleAsync<TResult>(IRequest<TResult> request, 
            CancellationToken token = default(CancellationToken));
    }
}
