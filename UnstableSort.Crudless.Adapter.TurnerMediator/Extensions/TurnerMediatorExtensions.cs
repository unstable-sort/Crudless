using System.Threading.Tasks;
using Turner.Infrastructure.Mediator;
using UnstableSort.Crudless.Requests;

namespace UnstableSort.Crudless.Adapter.TurnerMediator
{
    public static class TurnerMediatorExtensions
    {
        public static Task<Response> HandleCrudlessAsync(this IMediator mediator, Mediator.IRequest request)
        {
            return mediator.HandleAsync(request.Adapt());
        }

        public static Task<Response<T>> HandleCrudlessAsync<T>(this IMediator mediator, Mediator.IRequest<T> request)
        {
            return mediator.HandleAsync(request.Adapt());
        }
    }
}
