using System;
using UnstableSort.Crudless.Exceptions;

namespace UnstableSort.Crudless.Errors
{
    public class CreateEntityFailedError : RequestFailedError
    {
        public CreateEntityFailedError(object request, object item, Exception exception = null)
            : base(request, exception)
        {
            Item = item;
        }
        
        public new static bool IsReturnedFor(Exception e)
            => e is CreateEntityFailedException;

        public new static CreateEntityFailedError From(object request, Exception exception)
        {
            if (exception is CreateEntityFailedException cefException)
                return new CreateEntityFailedError(request, cefException.ItemProperty, cefException.InnerException);

            return new CreateEntityFailedError(request, null, exception);
        }

        public object Item { get; }
    }
}
