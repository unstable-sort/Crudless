using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UnstableSort.Crudless.Context;

namespace UnstableSort.Crudless.EntityFrameworkCore
{
    public class EntityFrameworkContextTransaction : IEntityContextTransaction
    {
        private IDbContextTransaction _transaction;

        private EntityFrameworkContextTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public static async Task<IEntityContextTransaction> BeginAsync(DbContext context, CancellationToken token = default(CancellationToken))
        {
            var transaction = await context.Database.BeginTransactionAsync(token);
            
            return new EntityFrameworkContextTransaction(transaction);
        }

        public Guid TransactionId => _transaction.TransactionId;

        public void Commit() => _transaction.Commit();

        public void Rollback() => _transaction.Rollback();

        public void Dispose()
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }
}
