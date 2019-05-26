using UnstableSort.Crudless.Transactions;

namespace UnstableSort.Crudless
{
    public static class IncludeTransactionInitializer
    {
        public static CrudlessInitializer UseTransactions(this CrudlessInitializer initializer)
        {
            return initializer.AddInitializer(new TransactionInitializer());
        }
    }
}
