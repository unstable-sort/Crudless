using System;
using UnstableSort.Crudless.Errors;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Tests.Fakes
{
    public class TestErrorHandler : ErrorHandler
    {
        protected override Response HandleError(FailedToFindError error)
        {
            throw new Exception(error.Reason);
        }
    }
}
