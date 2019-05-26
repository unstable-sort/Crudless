using UnstableSort.Crudless.Errors;

namespace UnstableSort.Crudless.Requests
{
    internal interface ICrudlessRequestHandler
    {
        ErrorDispatcher ErrorDispatcher { get; }
    }
}
