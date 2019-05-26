using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using UnstableSort.Crudless.Requests;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Validation
{
    internal class MaybeValidateAttribute : Attribute
    {
    }

    public class MaybeValidateDecoratorBase<TRequest, TResult>
    {
        private readonly ValidatorFactory _validatorFactory;

        public MaybeValidateDecoratorBase(ValidatorFactory validatorFactory)
        {
            _validatorFactory = validatorFactory;
        }

        public async Task<Response<TResult>> HandleAsync(TRequest request, Func<Task<Response<TResult>>> processRequest)
        {
            var validator = _validatorFactory.TryCreate<TRequest>();
            if (validator == null)
                return await processRequest();

            var errors = await validator.ValidateAsync(request);
            if (errors == null || errors.Count == 0)
                return await processRequest();
            
            return new Response<TResult>
            {
                Errors = Mapper.Map<List<Error>>(errors)
            };
        }
    }

    public class MaybeValidateDecorator<TRequest>
        : IRequestHandler<TRequest>
        where TRequest : IRequest, ICrudlessRequest
    {
        private readonly Func<IRequestHandler<TRequest>> _decorateeFactory;
        private readonly MaybeValidateDecoratorBase<TRequest, NoResult> _validationHandler;

        public MaybeValidateDecorator(Func<IRequestHandler<TRequest>> decorateeFactory,
            MaybeValidateDecoratorBase<TRequest, NoResult> validationHandler)
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

    public class MaybeValidateDecorator<TRequest, TResult>
        : IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>, ICrudlessRequest
    {
        private readonly Func<IRequestHandler<TRequest, TResult>> _decorateeFactory;
        private readonly MaybeValidateDecoratorBase<TRequest, TResult> _validationHandler;

        public MaybeValidateDecorator(Func<IRequestHandler<TRequest, TResult>> decorateeFactory,
            MaybeValidateDecoratorBase<TRequest, TResult> validationHandler)
        {
            _decorateeFactory = decorateeFactory;
            _validationHandler = validationHandler;
        }

        public Task<Response<TResult>> HandleAsync(TRequest request, CancellationToken token)
            => _validationHandler.HandleAsync(request, () => _decorateeFactory().HandleAsync(request, token));
    }
}
