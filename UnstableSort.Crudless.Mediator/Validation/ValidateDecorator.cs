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

    public class ValidateDecoratorBase<TRequest, TResult, TValidator>
        where TValidator : IRequestValidator<TRequest>
    {
        private readonly TValidator _validator;

        public ValidateDecoratorBase(TValidator validator)
        {
            _validator = validator;
        }

        public async Task<Response<TResult>> HandleAsync(TRequest request, Func<Task<Response<TResult>>> processRequest)
        {
            var errors = await _validator.ValidateAsync(request);
            if (errors == null || errors.Count == 0)
                return await processRequest();

            return new Response<TResult>
            {
                Errors = errors
                    .Select(x => new Error { PropertyName = x.PropertyName, ErrorMessage = x.ErrorMessage })
                    .ToList()
            };
        }
    }

    public class ValidateDecorator<TRequest, TValidator>
        : IRequestHandler<TRequest>
        where TRequest : IRequest
        where TValidator : IRequestValidator<TRequest>
    {
        private readonly Func<IRequestHandler<TRequest>> _decorateeFactory;
        private readonly ValidateDecoratorBase<TRequest, NoResult, TValidator> _validationHandler;

        public ValidateDecorator(Func<IRequestHandler<TRequest>> decorateeFactory,
            ValidateDecoratorBase<TRequest, NoResult, TValidator> validationHandler)
        {
            _decorateeFactory = decorateeFactory;
            _validationHandler = validationHandler;
        }

        public Task<Response> HandleAsync(TRequest request, CancellationToken token)
        {
            return _validationHandler
                .HandleAsync(request, () => _decorateeFactory().HandleAsync(request, token).ContinueWith(t => (Response<NoResult>)t.Result))
                .ContinueWith(t => (Response)t.Result);
        }
    }

    public class ValidateDecorator<TRequest, TResult, TValidator>
        : IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>
        where TValidator : IRequestValidator<TRequest>
    {
        private readonly Func<IRequestHandler<TRequest, TResult>> _decorateeFactory;
        private readonly ValidateDecoratorBase<TRequest, TResult, TValidator> _validationHandler;

        public ValidateDecorator(Func<IRequestHandler<TRequest, TResult>> decorateeFactory,
            ValidateDecoratorBase<TRequest, TResult, TValidator> validationHandler)
        {
            _decorateeFactory = decorateeFactory;
            _validationHandler = validationHandler;
        }

        public Task<Response<TResult>> HandleAsync(TRequest request, CancellationToken token)
            => _validationHandler.HandleAsync(request, () => _decorateeFactory().HandleAsync(request, token));
    }
}