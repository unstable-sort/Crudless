using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnstableSort.Crudless.Mediator
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class DoNotValidateAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ValidateAttribute : Attribute
    {
        public Type ValidatorType { get; }

        public ValidateAttribute(Type validatorType)
        {
            if (validatorType != null && (
                validatorType.IsGenericTypeDefinition ||
                !validatorType.GetInterfaces().Any(
                    x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequestValidator<>))))
            {
                string message = $"The type '{validatorType}' cannot be used for this attribute because " +
                                 $"it is not a concrete type implementing the IRequestValidator interface.";

                throw new Exception(message);
            }

            ValidatorType = validatorType;
        }

        public ValidateAttribute() : this(null)
        {
        }
    }

    public class ValidateDecorator<TRequest, TValidator>
        : IRequestHandler<TRequest>
        where TRequest : IRequest
        where TValidator : IRequestValidator<TRequest>
    {
        private readonly TValidator _validator;
        private readonly Func<IRequestHandler<TRequest>> _decorateeFactory;

        public ValidateDecorator(TValidator validator,
            Func<IRequestHandler<TRequest>> decorateeFactory)
        {
            _validator = validator;
            _decorateeFactory = decorateeFactory;
        }

        public async Task<Response> HandleAsync(TRequest request, CancellationToken token)
        {
            var errors = await _validator.ValidateAsync(request, token);

            if (errors != null && errors.Count > 0)
            {
                return new Response
                {
                    Errors = errors
                        .Select(x => new Error { PropertyName = x.PropertyName, ErrorMessage = x.ErrorMessage })
                        .ToList()
                };
            }

            return await _decorateeFactory().HandleAsync(request, token);
        }
    }

    public class ValidateDecorator<TRequest, TResult, TValidator>
        : IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>
        where TValidator : IRequestValidator<TRequest>
    {
        private readonly TValidator _validator;
        private readonly Func<IRequestHandler<TRequest, TResult>> _decorateeFactory;

        public ValidateDecorator(TValidator validator,
            Func<IRequestHandler<TRequest, TResult>> decorateeFactory)
        {
            _validator = validator;
            _decorateeFactory = decorateeFactory;
        }

        public async Task<Response<TResult>> HandleAsync(TRequest request, CancellationToken token)
        {
            var errors = await _validator.ValidateAsync(request, token);

            if (errors != null && errors.Count > 0)
            {
                return new Response<TResult>
                {
                    Errors = errors
                        .Select(x => new Error { PropertyName = x.PropertyName, ErrorMessage = x.ErrorMessage })
                        .ToList()
                };
            }

            return await _decorateeFactory().HandleAsync(request, token);
        }
    }
}