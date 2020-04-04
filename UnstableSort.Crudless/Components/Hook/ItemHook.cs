using System;
using System.Threading;
using System.Threading.Tasks;
using IServiceProvider = UnstableSort.Crudless.Common.ServiceProvider.IServiceProvider;

namespace UnstableSort.Crudless
{
    public interface IBoxedItemHook
    {
        Task<object> Run(object request, object item, CancellationToken ct = default(CancellationToken));
    }

    public interface IItemHookFactory
    {
        IBoxedItemHook Create(IServiceProvider provider);
    }

    public class FunctionItemHook
       : IBoxedItemHook
    {
        private readonly Func<object, object, CancellationToken, Task<object>> _hookFunc;

        public FunctionItemHook(Func<object, object, CancellationToken, Task<object>> hookFunc)
        {
            _hookFunc = hookFunc;
        }

        public Task<object> Run(object request, object item, CancellationToken ct = default(CancellationToken)) 
            => _hookFunc(request, item, ct);
    }
    
    public class FunctionItemHookFactory : IItemHookFactory
    {
        private readonly IBoxedItemHook _hook;

        private FunctionItemHookFactory(Func<object, object, CancellationToken, Task<object>> hook)
        {
            _hook = new FunctionItemHook(hook);
        }

        internal static FunctionItemHookFactory From<TRequest, TItem>(
            Func<TRequest, TItem, CancellationToken, Task<TItem>> hook)
        {
            return new FunctionItemHookFactory(
                (request, item, ct) => hook((TRequest)request, (TItem)item, ct)
                    .ContinueWith(t => (object)t.Result));
        }

        internal static FunctionItemHookFactory From<TRequest, TItem>(
            Func<TRequest, TItem, TItem> hook)
        {
            return new FunctionItemHookFactory(
                (request, item, ct) =>
                {
                    if (ct.IsCancellationRequested)
                        return Task.FromCanceled<object>(ct);

                    return Task.FromResult((object)hook((TRequest)request, (TItem)item));
                });
        }

        public IBoxedItemHook Create(IServiceProvider provider) => _hook;
    }

    public class InstanceItemHookFactory : IItemHookFactory
    {
        private readonly object _instance;
        private IBoxedItemHook _adaptedInstance;

        private InstanceItemHookFactory(object instance, IBoxedItemHook adaptedInstance)
        {
            _instance = instance;
            _adaptedInstance = adaptedInstance;
        }

        internal static InstanceItemHookFactory From<TRequest, TItem>(
            IItemHook<TRequest, TItem> hook)
        {
            return new InstanceItemHookFactory(
                hook,
                new FunctionItemHook((request, item, ct) 
                    => hook.Run((TRequest)request, (TItem)item, ct).ContinueWith(t => (object)t.Result)));
        }

        public IBoxedItemHook Create(IServiceProvider provider) => _adaptedInstance;
    }

    public class TypeItemHookFactory : IItemHookFactory
    {
        private Func<IServiceProvider, IBoxedItemHook> _hookFactory;

        public TypeItemHookFactory(Func<IServiceProvider, IBoxedItemHook> hookFactory)
        {
            _hookFactory = hookFactory;
        }
        
        internal static TypeItemHookFactory From<THook, TRequest, TItem>()
            where THook : IItemHook<TRequest, TItem>
        {
            return new TypeItemHookFactory(
                provider =>
                {
                    var instance = (IItemHook<TRequest, TItem>)provider.ProvideInstance(typeof(THook));
                    return new FunctionItemHook((request, item, ct)
                        => instance.Run((TRequest)request, (TItem)item, ct).ContinueWith(t => (object)t.Result));
                });
        }

        public IBoxedItemHook Create(IServiceProvider provider) => _hookFactory(provider);
    }
}
