using System;
using UnstableSort.Crudless.Exceptions;

namespace UnstableSort.Crudless.Errors
{
    public class CreateResultFailedError : RequestFailedError
    {
        public CreateResultFailedError(object request, object item, Exception exception = null)
            : base(request, exception)
        {
            Item = item;
        }
        
        public new static bool IsReturnedFor(Exception e)
            => e is CreateResultFailedException;

        public new static CreateResultFailedError From(object request, Exception exception)
        {
            if (exception is CreateResultFailedException crfException)
                return new CreateResultFailedError(request, crfException.EntityProperty, crfException.InnerException);

            return new CreateResultFailedError(request, null, exception);
        }

        public object Item { get; }
    }
}
