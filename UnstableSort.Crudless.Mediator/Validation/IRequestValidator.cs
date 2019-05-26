using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UnstableSort.Crudless.Mediator
{
    public interface IRequestValidator<TRequest>
    {
        Task<List<ValidationError>> ValidateAsync(TRequest request, CancellationToken token = default(CancellationToken));
    }
}
