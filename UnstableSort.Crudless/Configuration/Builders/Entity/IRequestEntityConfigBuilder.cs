// ReSharper disable once CheckNamespace
namespace UnstableSort.Crudless.Configuration.Builders
{
    public interface IRequestEntityConfigBuilder
    {
        void Build<TRequest>(RequestConfig<TRequest> config);
    }
}
