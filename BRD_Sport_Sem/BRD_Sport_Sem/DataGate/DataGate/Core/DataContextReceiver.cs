using Npgsql;

namespace DataGate.Core
{
    public class DataContextReceiver
    {
        private TransactionService _transaction;

        public DataContextReceiver(TransactionService transaction)
        {
            _transaction = transaction;
        }
        
        public DataContext GetDataContext(NpgsqlCommand readCommand, string name)
        {
            var dataContext = new DataContext(name, DataType.Table, null);
            var index = 0;
            _transaction.NpgsqlConnection.Open();
            using (var reader = readCommand.ExecuteReader())
                while (reader.Read())
                {
                    var rowContext = new DataContext(index.ToString(), DataType.Row, null);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var columnName = reader.GetName(i);
                        rowContext.Add(new DataContext(columnName, DataType.Field, reader.GetValue(i)));
                    }
                    dataContext.Add(rowContext);
                    index++;
                }
            _transaction.NpgsqlConnection.Close();

            return dataContext;
        }
    }
}