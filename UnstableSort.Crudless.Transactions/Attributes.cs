using System;

namespace UnstableSort.Crudless.Transactions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class NoTransactionAttribute : Attribute
    {
    }
}
