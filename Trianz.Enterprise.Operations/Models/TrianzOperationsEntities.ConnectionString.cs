using System;
using System.Configuration;
using System.Data.Entity;

namespace Trianz.Enterprise.Operations.Models
{
    public partial class TrianzOperationsEntities
    {
        public TrianzOperationsEntities(string connectionString)
            : base(connectionString)
        {
        }

        /// <summary>
        /// Returns the connection string from the environment variable DB_CONNECTION_STRING,
        /// falling back to the Web.config connectionStrings section.
        /// </summary>
        public static string GetConnectionString()
        {
            string envConn = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            if (!string.IsNullOrEmpty(envConn))
                return envConn;

            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["TrianzOperationsEntities"];
            if (settings != null && !string.IsNullOrEmpty(settings.ConnectionString))
                return settings.ConnectionString;

            return string.Empty;
        }

        /// <summary>
        /// Returns the ADO connection string from the environment variable ADO_CONNECTION_STRING,
        /// falling back to the Web.config connectionStrings section.
        /// </summary>        public static string GetAdoConnectionString()
        {
            string envConn = Environment.GetEnvironmentVariable("ADO_CONNECTION_STRING");
            if (!string.IsNullOrEmpty(envConn))
                return envConn;

            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["ADOConnectionString"];
            if (settings != null && !string.IsNullOrEmpty(settings.ConnectionString))
                return settings.ConnectionString;

            return string.Empty;
        }
    }
}
