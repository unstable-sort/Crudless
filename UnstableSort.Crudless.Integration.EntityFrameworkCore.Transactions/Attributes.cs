using System;

namespace UnstableSort.Crudless.Integration.EntityFrameworkCore.Transactions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class NoTransactionAttribute : Attribute
    {
    }
}
