using System.Linq;
using System.Reflection;

namespace UnstableSort.Crudless.Adapter.TurnerMediator
{
    internal static class Adapter
    {
        public static class For<TResult>
        {
            public static Turner.Infrastructure.Mediator.IRequest<TResult> Adapt(Mediator.IRequest<TResult> request)
            {
                var requestType = request.GetType();

                var adaptMethod = typeof(Adapter)
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                    .Single(x => x.Name == nameof(AdaptCrudless) && x.GetGenericArguments().Length == 2)
                    .MakeGenericMethod(requestType, typeof(TResult));

                return (Turner.Infrastructure.Mediator.IRequest<TResult>)
                    adaptMethod.Invoke(null, new object[] { request });
            }
        }

        public static Turner.Infrastructure.Mediator.IRequest Adapt(Mediator.IRequest request)
        {
            var requestType = request.GetType();

            var adaptMethod = typeof(Adapter)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Single(x => x.Name == nameof(AdaptCrudless) && x.GetGenericArguments().Length == 1)
                .MakeGenericMethod(request.GetType());

            return (Turner.Infrastructure.Mediator.IRequest)
                adaptMethod.Invoke(null, new object[] { request });
        }

        private static Turner.Infrastructure.Mediator.IRequest AdaptCrudless<TRequest>(TRequest request)
            where TRequest : Mediator.IRequest
        {
            return new CrudlessRequest<TRequest>(request);
        }

        private static Turner.Infrastructure.Mediator.IRequest<TResult> AdaptCrudless<TRequest, TResult>(TRequest request)
            where TRequest : Mediator.IRequest<TResult>
        {
            return new CrudlessRequest<TRequest, TResult>(request);
        }
    }
}
