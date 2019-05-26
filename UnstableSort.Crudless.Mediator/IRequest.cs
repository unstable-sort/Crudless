namespace UnstableSort.Crudless.Mediator
{
    public interface IRequest<T>
    {
    }

    public interface IRequest : IRequest<NoResult>
    {
    }
}