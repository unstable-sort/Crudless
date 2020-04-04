using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Validation
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    internal class MaybeValidateAttribute : Attribute
    {
    }
    
    public class MaybeValidateDecorator<TRequest>
        : IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        private readonly Func<IRequestHandler<TRequest>> _decorateeFactory;
        private readonly ValidatorFactory _validatorFactory;

        public MaybeValidateDecorator(Func<IRequestHandler<TRequest>> decorateeFactory,
            ValidatorFactory validatorFactory)
        {
            _decorateeFactory = decorateeFactory;
            _validatorFactory = validatorFactory;
        }

        public async Task<Response> HandleAsync(TRequest request, CancellationToken token)
        {
            var validator = _validatorFactory.TryCreate<TRequest>();
            if (validator == null)
                return await _decorateeFactory().HandleAsync(request, token);

            var errors = await validator.ValidateAsync(request);
            if (errors == null || errors.Count == 0)
                return await _decorateeFactory().HandleAsync(request, token);

            return new Response
            {
                Errors = Mapper.Map<List<Error>>(errors)
            };
        }
    }

    public class MaybeValidateDecorator<TRequest, TResult>
        : IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        private readonly Func<IRequestHandler<TRequest, TResult>> _decorateeFactory;
        private readonly ValidatorFactory _validatorFactory;

        public MaybeValidateDecorator(Func<IRequestHandler<TRequest, TResult>> decorateeFactory,
            ValidatorFactory validatorFactory)
        {
            _decorateeFactory = decorateeFactory;
            _validatorFactory = validatorFactory;
        }

        public async Task<Response<TResult>> HandleAsync(TRequest request, CancellationToken token)
        {
            var validator = _validatorFactory.TryCreate<TRequest>();
            if (validator == null)
                return await _decorateeFactory().HandleAsync(request, token);

            var errors = await validator.ValidateAsync(request);
            if (errors == null || errors.Count == 0)
                return await _decorateeFactory().HandleAsync(request, token);

            return new Response<TResult>
            {
                Errors = Mapper.Map<List<Error>>(errors)
            };
        }
    }
}
