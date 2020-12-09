using Npgsql;

namespace DataGate.Core
{
    public class Transaction
    {
        public CommandBuilder CommandBuilder;
        public NpgsqlConnection NpgsqlConnection;

        public Transaction(NpgsqlConnection connection)
        {
            NpgsqlConnection = connection;
            CommandBuilder = new CommandBuilder(connection);
        }
    }
}