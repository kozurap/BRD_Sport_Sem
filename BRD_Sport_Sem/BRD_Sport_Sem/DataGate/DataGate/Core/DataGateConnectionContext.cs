using System;
using System.Data;
using Npgsql;

namespace DataGate.Core
{
    public class DataGateConnectionContext
    {
        public readonly string ConnectionString;
        public readonly DataGateRegistry Registry;

        public DataGateConnectionContext(string connectionString)
        {
            ConnectionString = connectionString;
            Registry = new DataGateRegistry();
        }
    }
}