using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DataGate.Core;
using Npgsql;

namespace DataGate.Core
{
    public class DataGateORM
    {
        internal static DataGateConnectionContext ConnectionContext { get; private set; }
        internal TransactionService TransactionService;
        #region Decorators for TransactionService
        internal Transaction CurrentTransaction => TransactionService.CurrentTransaction;

        internal void InitializeTransaction() => TransactionService.InitializeTransaction();

        internal void EndTransaction() => TransactionService.EndTransaction();
        #endregion

        internal readonly DataContextReceiver Receiver;
        internal readonly DataContextExecutor Executor;
        
        public DataGateORM(TransactionService transactionService,
            DataContextReceiver receiver, 
            DataContextExecutor executor)
        {
            TransactionService = transactionService;
            Receiver = receiver;
            Executor = executor;
        }
        
        public static void Connect(string connectionString)
        {
            ConnectionContext = new DataGateConnectionContext(connectionString);
        }

        public static void Register<T>(string tableName)
        {
            if(ConnectionContext == null)
                throw new Exception("Connection not established");
            
            var connection = new NpgsqlConnection(ConnectionContext.ConnectionString);

            var checkCommand = new NpgsqlCommand(
                $"SELECT count(*) FROM information_schema.columns WHERE table_name = '{tableName}' AND column_name = 'datagate_id';",
                connection
                );
            
            connection.Open();
            long res = (long)checkCommand.ExecuteScalar();
            connection.Close();

            if (res == 0)
            {
                var command = new NpgsqlCommand(
                    $"ALTER TABLE {tableName} ADD COLUMN IF NOT EXISTS datagate_id SERIAL;"
                    , connection);

                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            
            connection.Dispose();

            ConnectionContext.Registry.RegisterRelationship<T>(
                new TableTypeRelationship(tableName, typeof(T))
            );
        }

        public QueryBuilder<T, T> Get<T>()
        {
            if (ConnectionContext == null)
                throw new Exception("Connection not established");

            return new QueryBuilder<T, T>(this);
        }
        
        public int Insert<T>(T obj)
        {
            if(ConnectionContext == null)
                throw new Exception("Connection not established");
            
            var relationship = ConnectionContext.Registry.GetRelationship(typeof(T));
            var context = relationship.ToData(new[] {obj});
            InitializeTransaction();
            Executor.Insert(context, relationship);
            var result = CurrentTransaction.CommandBuilder.ExecuteScalar();
            EndTransaction();
            return (int)result[0];
        }
        
        public int[] Insert<T>(IEnumerable<T> objs)
        {
            if(ConnectionContext == null)
                throw new Exception("Connection not established");
            
            var relationship = ConnectionContext.Registry.GetRelationship(typeof(T));
            var context = relationship.ToData(objs);
            InitializeTransaction();
            Executor.Insert(context, relationship);
            var result = CurrentTransaction.CommandBuilder.ExecuteScalar();
            EndTransaction();
            return result.Cast<int>().ToArray();
        }

        public void Remove<T>(int datagate_id)
        {
            if(ConnectionContext == null)
                throw new Exception("Connection not established");
            
            var execute = CurrentTransaction == null;
            if(execute)
                InitializeTransaction();

            var relationship = ConnectionContext.Registry.GetRelationship<T>();
            var command = new NpgsqlCommand($"DELETE FROM {relationship.TableName} WHERE datagate_id = @datagate_id");
            command.Parameters.AddWithValue("datagate_id", datagate_id);
            CurrentTransaction.CommandBuilder.AppendQuery(command);
            
            if (execute)
            {
                CurrentTransaction.CommandBuilder.ExecuteNonQuery();
                EndTransaction();
            }
        }
    }
}