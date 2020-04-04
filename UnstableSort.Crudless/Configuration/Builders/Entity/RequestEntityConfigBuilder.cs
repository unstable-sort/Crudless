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
        public RequestEntityConfigBuilder<TRequest, TEntity> UseRequestKey<TKey>(
            Expression<Func<TRequest, TKey>> requestItemKeyExpr)
        {
            RequestItemKeys = Key.MakeKeys(requestItemKeyExpr);

            return this;
        }

        public RequestEntityConfigBuilder<TRequest, TEntity> UseRequestKey(string requestKeyMember)
        {
            RequestItemKeys = new[] { Key.MakeKey<TRequest>(requestKeyMember) };

            return this;
        }

        public RequestEntityConfigBuilder<TRequest, TEntity> UseRequestKey(string[] requestKeyMembers)
        {
            RequestItemKeys = requestKeyMembers.Select(Key.MakeKey<TRequest>).ToArray();

            return this;
        }

        public RequestEntityConfigBuilder<TRequest, TEntity> CreateEntityWith(
            Func<RequestContext<TRequest>, CancellationToken, Task<TEntity>> creator)
        {
            CreateEntity = (context, item, ct) => creator(context.Cast<TRequest>(), ct);

            return this;
        }

        public RequestEntityConfigBuilder<TRequest, TEntity> CreateEntityWith(
            Func<RequestContext<TRequest>, Task<TEntity>> creator)
            => CreateEntityWith((context, ct) => creator(context));

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

        public RequestEntityConfigBuilder<TRequest, TEntity> UpdateEntityWith(
            Func<RequestContext<TRequest>, TEntity, CancellationToken, Task<TEntity>> updator)
        {
            UpdateEntity = (context, item, entity, ct) => updator(context.Cast<TRequest>(), entity, ct);

            return this;
        }

        public RequestEntityConfigBuilder<TRequest, TEntity> UpdateEntityWith(
            Func<RequestContext<TRequest>, TEntity, Task<TEntity>> updator)
            => UpdateEntityWith((context, entity, ct) => updator(context, entity));

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

        public override void Build<TCompatibleRequest>(RequestConfig<TCompatibleRequest> config)
        {
            base.Build(config);

            if (Selector == null)
                DefaultSelector(config);
        }

        private void DefaultSelector<TCompatibleRequest>(
            RequestConfig<TCompatibleRequest> config)
        {
            var requestKeys = config.GetRequestKeys();
            var entityKeys = config.GetKeysFor<TEntity>();

            if (requestKeys != null && requestKeys.Length > 0 &&
                entityKeys != null && requestKeys.Length > 0)
            {
                if (requestKeys.Length != entityKeys.Length)
                    throw new BadConfigurationException($"Incompatible keys defined for '{typeof(TCompatibleRequest)}' and '{typeof(TEntity)}'");

                var builder = new SelectorBuilder<TRequest, TEntity>();
                config.SetEntitySelector<TEntity>(builder.Single(requestKeys.Zip(entityKeys, (r, e) => (r, e))));
            }
        }
    }
}
