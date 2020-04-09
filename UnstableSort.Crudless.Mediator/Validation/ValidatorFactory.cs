using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.Mediator
{
    public class ValidatorFactory
    {
        private readonly ServiceProviderContainer _provider;

        public ValidatorFactory(ServiceProviderContainer provider)
        {
            _provider = provider;
        }

        public IRequestValidator<TRequest> TryCreate<TRequest>()
        {
            try
            {
                return _provider
                    .GetProvider()
                    .ProvideInstance<IRequestValidator<TRequest>>();
            }
            catch (FailedToCreateServiceException)
            {
                return null;
            }
        }
    }
}
