using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ESAT.Import.Utils
{
    public class PgDbConnector
    {
        private Dictionary<string, NpgsqlConnection> connections = new Dictionary<string, NpgsqlConnection>();

        public Dictionary<string, NpgsqlConnection> Connections { get => connections; set => connections = value; }

        public NpgsqlConnection GetConnection(string credential)
        {
            if (Connections.ContainsKey(credential))
            {
                if (Connections[credential].State != ConnectionState.Open)
                {
                    Connections[credential].Open();
                }

                return Connections[credential];
            } else
            {
                NpgsqlConnection connection = new NpgsqlConnection(credential);
                connection.Open();
                Connections[credential] = connection;
                return connection;
            }
        }

    }
}
