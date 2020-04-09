using System;

namespace UnstableSort.Crudless.Common.ServiceProvider
{
    public interface IServiceProvider : IDisposable
    {
        TService ProvideInstance<TService>()
            where TService : class;

        object ProvideInstance(Type type);
    }
}
