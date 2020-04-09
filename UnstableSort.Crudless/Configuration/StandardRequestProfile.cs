using UnstableSort.Crudless.Configuration.Builders;
using UnstableSort.Crudless.Exceptions;
using UnstableSort.Crudless.Requests;

namespace UnstableSort.Crudless.Configuration
{
    public abstract class RequestProfile<TRequest>
        : RequestProfileCommon<TRequest>
    {
        public RequestProfile()
        {
            if (typeof(IBulkRequest).IsAssignableFrom(typeof(TRequest)) &&
                !typeof(TRequest).IsInterface &&
                !typeof(TRequest).IsAbstract &&
                !typeof(TRequest).IsGenericTypeDefinition)
            {
                var message =
                    $"Unable to build configuration for request '{typeof(TRequest)}'." +
                    $"This request type should define a 'BulkRequestProfile'.";

                throw new BadConfigurationException(message);
            }
        }

        /// <summary>
        /// Begins a configuration for an entity type.
        /// See the docs for more information on configuring entities for requests.
        /// </summary>
        public RequestEntityConfigBuilder<TRequest, TEntity> ForEntity<TEntity>()
            where TEntity : class
        {
            var builder = new RequestEntityConfigBuilder<TRequest, TEntity>();
            _requestEntityBuilders[typeof(TEntity)] = builder;

            return builder;
        }
    }

    public class DefaultRequestProfile<TRequest> : RequestProfile<TRequest>
        where TRequest : ICrudlessRequest
    {
    }
}
