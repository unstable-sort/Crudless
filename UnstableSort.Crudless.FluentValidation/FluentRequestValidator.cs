using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.FluentValidation
{
    public class FluentRequestValidator<TRequest> : IRequestValidator<TRequest>
    {
        private readonly IValidator<TRequest> _validator;

        public FluentRequestValidator(IValidator<TRequest> validator)
        {
            _validator = validator;
        }

        public async Task<List<ValidationError>> ValidateAsync(TRequest request, 
            CancellationToken token = default(CancellationToken))
        {
            var validationResult = await _validator.ValidateAsync(request, token);
            if (validationResult.IsValid)
                return null;

            return validationResult.Errors
                .Select(error => new ValidationError
                {
                    ErrorMessage = error.ErrorMessage,
                    PropertyName = error.PropertyName
                })
                .ToList();
        }
    }
}
