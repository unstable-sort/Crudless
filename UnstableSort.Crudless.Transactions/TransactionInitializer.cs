using System.Reflection;
using SimpleInjector;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Transactions
{
    public class TransactionInitializer : ICrudlessInitializationTask
    {
        public void Run(Container container, Assembly[] assemblies, CrudlessOptions options)
        {
            container.RegisterDecorator(typeof(IRequestHandler<>), typeof(TransactionDecorator<>));
            container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(TransactionDecorator<,>));
        }
    }
}
