using System.Threading.Tasks;
using UnstableSort.Crudless.Adapter.TurnerMediator;

namespace Turner.Infrastructure.Mediator
{
    public static class TurnerMediatorExtensions
    {
        public static Task<Response> HandleAsync(this IMediator mediator, UnstableSort.Crudless.Mediator.IRequest request)
        {
            return mediator.HandleAsync(Adapter.Adapt(request));
        }

        public static Task<Response<T>> HandleAsync<T>(this IMediator mediator, UnstableSort.Crudless.Mediator.IRequest<T> request)
        {
            return mediator.HandleAsync(Adapter.For<T>.Adapt(request));
        }
    }
}
