using System;
using SimpleInjector;

namespace UnstableSort.Crudless.Mediator
{
    public class ValidatorFactory
    {
        private readonly Func<Type, object> _validatorCreator;

        public ValidatorFactory(Func<Type, object> creator)
        {
            _validatorCreator = creator;
        }

        public IRequestValidator<TRequest> TryCreate<TRequest>()
        {
            try
            {
                return (IRequestValidator<TRequest>)_validatorCreator(typeof(IRequestValidator<TRequest>));
            }
            catch (ActivationException)
            {
                return null;
            }
        }
    }
}
