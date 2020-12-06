using System;
using Npgsql;

namespace DataGate.Core
{
    public class TransactionService : IDisposable
    {
        public NpgsqlConnection NpgsqlConnection;
        public Transaction CurrentTransaction { get; private set; }

        public TransactionService()
        {
            NpgsqlConnection = 
                new NpgsqlConnection(DataGateORM.ConnectionContext.ConnectionString);
        }

        public void InitializeTransaction()
        {
            CurrentTransaction = new Transaction(NpgsqlConnection);
        }

        public void EndTransaction()
        {
            CurrentTransaction = null;
        }

        public void Dispose()
        {
            NpgsqlConnection?.Dispose();
        }
    }
}