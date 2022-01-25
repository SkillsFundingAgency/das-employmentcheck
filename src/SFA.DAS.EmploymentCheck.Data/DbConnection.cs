using System;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Data
{
    public class DbConnection
    {
        private const string AzureResource = "https://database.windows.net/";
        
        public async Task<SqlConnection> CreateSqlConnection(
            ApplicationSettings applicationSettings,
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            VerifyConnectionString(applicationSettings.DbConnectionString);

            var sqlConnection = new SqlConnection(applicationSettings.DbConnectionString);

            if (azureServiceTokenProvider != null && applicationSettings.EnvironmentName != "LOCAL")
            {
                sqlConnection.AccessToken = await azureServiceTokenProvider.GetAccessTokenAsync(AzureResource);
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