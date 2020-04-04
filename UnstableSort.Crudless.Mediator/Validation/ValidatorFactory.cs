using UnstableSort.Crudless.Common.ServiceProvider;

namespace UnstableSort.Crudless.Mediator
{
    public class ValidatorFactory
    {
        private readonly IServiceProvider _provider;

        public ValidatorFactory(ServiceProviderContainer container)
        {
            _provider = container.CreateProvider();
        }

        public IRequestValidator<TRequest> TryCreate<TRequest>()
        {
            try
            {
                return _provider.ProvideInstance<IRequestValidator<TRequest>>();
            }
            catch (FailedToCreateServiceException)
            {
                return null;
            }
        }
    }
}
