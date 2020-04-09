using System;
using System.Collections.Generic;
using System.Linq;
using IServiceProvider = UnstableSort.Crudless.Common.ServiceProvider.IServiceProvider;

namespace UnstableSort.Crudless.Configuration
{
    public abstract class HookConfig<THookFactory, THook>
    {
        private readonly Func<THookFactory, IServiceProvider, THook> _hookFactoryCreateFunc;

        private List<THookFactory> _hookFactories = new List<THookFactory>();

        protected HookConfig(Func<THookFactory, IServiceProvider, THook> hookFactoryCreateFunc)
        {
            _hookFactoryCreateFunc = hookFactoryCreateFunc;
        }

        internal void SetHooks(List<THookFactory> hookFactories)
        {
            _hookFactories = hookFactories;
        }

        internal void AddHooks(IEnumerable<THookFactory> hookFactories)
        {
            _hookFactories.AddRange(hookFactories);
        }

        public List<THook> GetHooks(IServiceProvider provider) 
            => _hookFactories.Select(factory => _hookFactoryCreateFunc(factory, provider)).ToList();
    }

    public class RequestHookConfig
        : HookConfig<IRequestHookFactory, IBoxedRequestHook>
    {
        public RequestHookConfig() : base((factory, provider) => factory.Create(provider))
        {
        }
    }

    public class EntityHookConfig
        : HookConfig<IEntityHookFactory, IBoxedEntityHook>
    {
        public EntityHookConfig() : base((factory, provider) => factory.Create(provider))
        {
        }
    }

    public class ItemHookConfig
        : HookConfig<IItemHookFactory, IBoxedItemHook>
    {
        public ItemHookConfig() : base((factory, provider) => factory.Create(provider))
        {
        }
    }

    public class ResultHookConfig
        : HookConfig<IResultHookFactory, IBoxedResultHook>
    {
        public ResultHookConfig() : base((factory, provider) => factory.Create(provider))
        {
        }
    }

    public class AuditHookConfig
        : HookConfig<IAuditHookFactory, IBoxedAuditHook>
    {
        public AuditHookConfig() : base((factory, provider) => factory.Create(provider))
        {
        }
    }
}
