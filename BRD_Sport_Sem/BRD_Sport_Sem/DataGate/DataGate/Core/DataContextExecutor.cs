using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;

namespace DataGate.Core
{
    public class DataContextExecutor
    {
        private TransactionService _transaction;

        public DataContextExecutor(TransactionService transaction)
        {
            _transaction = transaction;
        }
        
        private static ConcurrentDictionary<string, NpgsqlCommand> _insertCommands
            = new ConcurrentDictionary<string, NpgsqlCommand>();

        public void Insert(DataContext context, TableTypeRelationship relationship) //TODO: Fix: Executes only first command, other ignored
        {
            var command = _insertCommands.ContainsKey(context.Name) ? _insertCommands[context.Name] : null;
            foreach (var row in context)
            {
                if (command == null)
                {
                    var commandStringBuilder = new StringBuilder($"INSERT INTO {context.Name} (");
                    BuildNames(commandStringBuilder, row.Value, name => name, ")");

                    commandStringBuilder.Append(" VALUES (");

                    BuildNames(commandStringBuilder, row.Value, name => $"@{name}", ")");
                    
                    commandStringBuilder.Append($" RETURNING {relationship.Id}");
                    command = new NpgsqlCommand(commandStringBuilder.ToString()
                        , _transaction.NpgsqlConnection);
                }
                
                foreach (var field in row.Value)
                    command.Parameters.AddWithValue(field.Value.Name, field.Value.Value ?? DBNull.Value);
                
                _transaction.CurrentTransaction.CommandBuilder.AppendQuery(command);
            }
        }

        public void CompareAndUpdate(DataContext current, DataContext old)
        {
            if (current.Name != old.Name)
                return;

            foreach (var (currentRow, oldRow) in current.Join(old, currentRow => currentRow.Value.Name,
                oldRow => oldRow.Value.Name, (currentRow, oldRow) => (currentRow.Value, oldRow.Value)))
            {
                var diffFields = currentRow.Join(oldRow,
                        currentField => currentField.Value.Name,
                        oldField => oldField.Value.Name,
                        (currentField, oldField) => (currentField.Value, oldField.Value))
                    .Where(fields => !fields.Item1.Value.Equals(fields.Item2.Value)).ToList();

                if (diffFields.Count != 0)
                {
                    var commandBuilder = new StringBuilder($"UPDATE {current.Name} SET ");
                    var command = new NpgsqlCommand();
                    var index = 0;

                    foreach (var (currentField, oldField) in diffFields)
                    {
                        commandBuilder.Append($"{currentField.Name} = @{currentField.Name}");
                        command.Parameters.AddWithValue(currentField.Name, currentField.Value ?? DBNull.Value);
                        if (index != diffFields.Count - 1)
                            commandBuilder.Append(", ");
                        index++;
                    }

                    commandBuilder.Append($" WHERE datagate_id = @datagate_id;");
                    command.Parameters.AddWithValue("datagate_id", currentRow["datagate_id"].Value);
                    command.CommandText = commandBuilder.ToString();
                    _transaction.CurrentTransaction.CommandBuilder.AppendQuery(command);
                }
            }
        }
        
        private void BuildNames(StringBuilder builder, DataContext row
            , Func<string, string> nameFactory, string endWith,
            string separator = ", ", bool excludeDatagateId = false)
        {
            var index = 0;
            foreach (var field in row)
            {
                if (!excludeDatagateId || field.Value.Name != "datagate_id")
                {
                    builder.Append(nameFactory(field.Value.Name));
                    if (index != row.Count - 1)
                        builder.Append(separator);
                    else builder.Append(endWith);
                }

                index++;
            }
        }
    }
}