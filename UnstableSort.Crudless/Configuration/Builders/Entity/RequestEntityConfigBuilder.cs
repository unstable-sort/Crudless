using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UnstableSort.Crudless.Configuration.Builders.Select;
using UnstableSort.Crudless.Exceptions;
using UnstableSort.Crudless.Requests;

// ReSharper disable once CheckNamespace
namespace UnstableSort.Crudless.Configuration.Builders
{
    public class RequestEntityConfigBuilder<TRequest, TEntity>
        : RequestEntityConfigBuilderCommon<TRequest, TEntity, RequestEntityConfigBuilder<TRequest, TEntity>>
        where TEntity : class
    {
        /// <summary>
        /// Applies this profile to the provided configuration.
        /// This method is not intended to be used externally.
        /// </summary>
        public override void Build<TCompatibleRequest>(RequestConfig<TCompatibleRequest> config)
        {
            base.Build(config);

            if (Selector == null)
            {
                try
                {
                    BuildDefaultSelector(config);
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Provides a request's "key" members.
        /// </summary>
        public RequestEntityConfigBuilder<TRequest, TEntity> UseRequestKey<TKey>(
            Expression<Func<TRequest, TKey>> requestItemKeyExpr)
        {
            RequestItemKeys = Key.MakeKeys(requestItemKeyExpr);

            return this;
        }

        /// <summary>
        /// Provides a request's "key" member.
        /// </summary>
        public RequestEntityConfigBuilder<TRequest, TEntity> UseRequestKey(string requestKeyMember)
        {
            RequestItemKeys = new[] { Key.MakeKey<TRequest>(requestKeyMember) };

            return this;
        }

        /// <summary>
        /// Provides a request's "key" members.
        /// </summary>
        public RequestEntityConfigBuilder<TRequest, TEntity> UseRequestKey(string[] requestKeyMembers)
        {
            RequestItemKeys = requestKeyMembers.Select(Key.MakeKey<TRequest>).ToArray();

            return this;
        }

        /// <summary>
        /// Provides request handlers with how to create a new entity from a request.
        /// The default method is to resolve an IMapper and map the request into a new TEntity.
        /// </summary>
        public RequestEntityConfigBuilder<TRequest, TEntity> CreateEntityWith(
            Func<RequestContext<TRequest>, CancellationToken, Task<TEntity>> creator)
        {
            CreateEntity = (context, item, ct) => creator(context.Cast<TRequest>(), ct);

            return this;
        }

        /// <summary>
        /// Provides request handlers with how to create a new entity from a request.
        /// The default method is to resolve an IMapper and map the request into a new TEntity.
        /// </summary>
        public RequestEntityConfigBuilder<TRequest, TEntity> CreateEntityWith(
            Func<RequestContext<TRequest>, Task<TEntity>> creator)
            => CreateEntityWith((context, ct) => creator(context));

        /// <summary>
        /// Provides request handlers with how to create a new entity from a request.
        /// The default method is to resolve an IMapper and map the request into a new TEntity.
        /// </summary>
        public RequestEntityConfigBuilder<TRequest, TEntity> CreateEntityWith(Func<RequestContext<TRequest>, TEntity> creator)
        {
            CreateEntity = (context, item, ct) =>
            {
                if (ct.IsCancellationRequested)
                    return Task.FromCanceled<TEntity>(ct);

                return Task.FromResult(creator(context.Cast<TRequest>()));
            };

            return this;
        }

        /// <summary>
        /// Provides request handlers with how to update an entity from a request.
        /// The default method is to resolve an IMapper and map the request on to the existing entity.
        /// </summary>
        public RequestEntityConfigBuilder<TRequest, TEntity> UpdateEntityWith(
            Func<RequestContext<TRequest>, TEntity, CancellationToken, Task<TEntity>> updator)
        {
            UpdateEntity = (context, item, entity, ct) => updator(context.Cast<TRequest>(), entity, ct);

            return this;
        }

        /// <summary>
        /// Provides request handlers with how to update an entity from a request.
        /// The default method is to resolve an IMapper and map the request on to the existing entity.
        /// </summary>
        public RequestEntityConfigBuilder<TRequest, TEntity> UpdateEntityWith(
            Func<RequestContext<TRequest>, TEntity, Task<TEntity>> updator)
            => UpdateEntityWith((context, entity, ct) => updator(context, entity));

        /// <summary>
        /// Provides request handlers with how to update an entity from a request.
        /// The default method is to resolve an IMapper and map the request on to the existing entity.
        /// </summary>
        public RequestEntityConfigBuilder<TRequest, TEntity> UpdateEntityWith(
            Func<RequestContext<TRequest>, TEntity, TEntity> updator)
        {
            UpdateEntity = (context, item, entity, ct) =>
            {
                if (ct.IsCancellationRequested)
                    return Task.FromCanceled<TEntity>(ct);

                return Task.FromResult(updator(context.Cast<TRequest>(), entity));
            };

            return this;
        }

        private void BuildDefaultSelector<TCompatibleRequest>(
            RequestConfig<TCompatibleRequest> config)
        {
            var kRequest = config.GetRequestKeys();
            var kEntity = config.GetKeysFor<TEntity>();

            if (kRequest != null && kRequest.Length > 0 && kEntity != null && kRequest.Length > 0)
            {
                if (kRequest.Length != kEntity.Length)
                    throw new IncompatibleKeysException(typeof(TCompatibleRequest), typeof(TEntity));

                var selector = SelectorHelpers.BuildSingle<TRequest, TEntity>(kRequest.Zip(kEntity, (r, e) => (r, e)));

                config.SetEntitySelector<TEntity>(selector);
            }
        }
    }
}
