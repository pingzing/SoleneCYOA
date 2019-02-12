using Microsoft.Extensions.Logging;
using Solene.Database;
using System;

namespace Solene.Backend
{
    public class Database
    {
        public static SoleneTableClient GetDatabaseClient(ILogger logger)
        {
            string connectionString = Environment.GetEnvironmentVariable("SOLENE_CONNECTION_STRING", EnvironmentVariableTarget.Process);
            return new SoleneTableClient(connectionString, logger);
        }
    }
}
