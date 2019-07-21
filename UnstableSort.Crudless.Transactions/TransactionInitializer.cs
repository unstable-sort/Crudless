using System.Reflection;
using SimpleInjector;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Transactions
{
    public class TransactionInitializer : CrudlessInitializationTask
    {
        private readonly TransactionType _transactionType;

        public TransactionInitializer(TransactionType transactionType)
        {
            _transactionType = transactionType;
        }

        public override void Run(Container container, Assembly[] assemblies, CrudlessOptions options)
        {
            switch(_transactionType)
            {
                default:
                case TransactionType.TransactionScope:
                    container.RegisterDecorator(typeof(IRequestHandler<>), typeof(TransactionScopeTransactionDecorator<>));
                    container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(TransactionScopeTransactionDecorator<,>));
                    break;

                case TransactionType.EntityFramework:
                    container.RegisterDecorator(typeof(IRequestHandler<>), typeof(EntityFrameworkTransactionDecorator<>));
                    container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(EntityFrameworkTransactionDecorator<,>));
                    break;
            }
        }

        public override bool Supports(string option)
        {
            if (option == "Transactions")
                return true;

            return base.Supports(option);
        }
    }
}
