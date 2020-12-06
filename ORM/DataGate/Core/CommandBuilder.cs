using System.Collections.Generic;
using Npgsql;

namespace DataGate.Core
{
    public class CommandBuilder
    {
        public NpgsqlConnection NpgsqlConnection;
        private List<NpgsqlCommand> _commands;

        public CommandBuilder(NpgsqlConnection connection)
        {
            _commands = new List<NpgsqlCommand>();
            NpgsqlConnection = connection;
        }

        public void AppendQuery(NpgsqlCommand query)
        {
            if(query.Connection == null) 
                query.Connection = NpgsqlConnection;
            _commands.Add(query);
        }

        public void ExecuteNonQuery()
        {
            NpgsqlConnection.Open();
            foreach (var command in _commands)
                command.ExecuteNonQuery();
            NpgsqlConnection.Close();
        }
        
        public List<object> ExecuteScalar()
        {
            var result = new List<object>();
            NpgsqlConnection.Open();
            foreach (var command in _commands)
                result.Add(command.ExecuteScalar());
            NpgsqlConnection.Close();
            return result;
        }
    }
}