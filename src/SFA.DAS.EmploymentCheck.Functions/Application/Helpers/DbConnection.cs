using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Helpers
{
    public class DbConnection
    {
        public async Task<SqlConnection> CreateSqlConnection(
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            VerifyConnectionString(connectionString);

            var sqlConnection = new SqlConnection(connectionString);

            if (!string.IsNullOrEmpty(azureResource))
            {
                sqlConnection.AccessToken = await azureServiceTokenProvider.GetAccessTokenAsync(azureResource);
            }

            return sqlConnection;
        }

        private static void VerifyConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException(
                    $"{nameof(DbConnection)}.CreateSqlConnection: Missing SQL connection string for the Employment Check Database.");
            }
        }
    }
}