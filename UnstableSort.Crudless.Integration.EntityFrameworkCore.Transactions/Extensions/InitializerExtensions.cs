using UnstableSort.Crudless.Integration.EntityFrameworkCore.Transactions;

namespace UnstableSort.Crudless
{
    public static class IncludeTransactionInitializer
    {
        public static CrudlessInitializer UseTransactions(this CrudlessInitializer initializer, 
            TransactionType transactionType = TransactionType.Auto)
        {
            if (transactionType == TransactionType.Auto)
            {
                transactionType = initializer.Supports("EntityFramework")
                    ? TransactionType.EntityFramework
                    : TransactionType.TransactionScope;
            }

            return initializer.AddInitializer(new TransactionInitializer(transactionType));
        }
    }
}
