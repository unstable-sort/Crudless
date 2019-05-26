using System;

namespace UnstableSort.Crudless.Errors
{
    public class CrudlessError
    {
        public CrudlessError(Exception exception = null)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}
