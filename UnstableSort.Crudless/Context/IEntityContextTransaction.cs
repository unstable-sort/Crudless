using System;

namespace UnstableSort.Crudless.Context
{
    public interface IEntityContextTransaction : IDisposable
    {
        Guid TransactionId { get; }

        void Commit();

        void Rollback();
    }
}
