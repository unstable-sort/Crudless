namespace UnstableSort.Crudless.Requests
{
    public static class CrudlessRequestExtensions
    {
        public static Turner.Infrastructure.Mediator.IRequest Adapt(this Mediator.IRequest request)
        {
            return Adapter.TurnerMediator.Adapter.Adapt(request);
        }

        public static Turner.Infrastructure.Mediator.IRequest<TResult> Adapt<TResult>(this Mediator.IRequest<TResult> request)
        {
            return Adapter.TurnerMediator.Adapter.For<TResult>.Adapt(request);
        }
    }
}
