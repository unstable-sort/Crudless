using System;
using System.Reflection;
using UnstableSort.Crudless.Common.ServiceProvider;
using UnstableSort.Crudless.Mediator;

namespace UnstableSort.Crudless.Integration.EntityFrameworkCore.Transactions
{
    public class TransactionInitializer : CrudlessInitializationTask
    {
        private readonly TransactionType _transactionType;

        public TransactionInitializer(TransactionType transactionType)
        {
            _transactionType = transactionType;
        }

        public override void Run(ServiceProviderContainer container, Assembly[] assemblies, CrudlessOptions options)
        {
            switch(_transactionType)
            {
                default:
                case TransactionType.TransactionScope:
                    container.RegisterDecorator(typeof(IRequestHandler<>), typeof(TransactionScopeTransactionDecorator<>), ShouldDecorate());
                    container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(TransactionScopeTransactionDecorator<,>), ShouldDecorate());
                    break;

                case TransactionType.EntityFramework:
                    container.RegisterDecorator(typeof(IRequestHandler<>), typeof(EntityFrameworkTransactionDecorator<>), ShouldDecorate());
                    container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(EntityFrameworkTransactionDecorator<,>), ShouldDecorate());
                    break;
            }
        }

        public override bool Supports(string option)
        {
            if (option == "Transactions")
                return true;

            return base.Supports(option);
        }

        private static Predicate<DecoratorConditionalContext> ShouldDecorate()
        {
            return c => !c.ImplementationType.RequestHasAttribute(typeof(NoTransactionAttribute));
        }
    }
}
