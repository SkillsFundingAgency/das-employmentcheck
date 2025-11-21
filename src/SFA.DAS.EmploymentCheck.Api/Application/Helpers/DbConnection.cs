using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Api.Application.Helpers
{
    [ExcludeFromCodeCoverage]
    public class DbConnection
    {
        private const string AzureSqlScope = "https://database.windows.net/.default";

        public async Task<SqlConnection> CreateSqlConnection(
            string connectionString,
            Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider azureServiceTokenProvider = null)
        {
            VerifyConnectionString(connectionString);

            var sqlConnection = new SqlConnection(connectionString);

            if (ContainsSqlUserPassword(connectionString))
            {
                await sqlConnection.OpenAsync();
                return sqlConnection;
            }

            string accessToken;
            if (azureServiceTokenProvider != null)
            {
                accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://database.windows.net/");
            }
            else
            {
                var credential = new DefaultAzureCredential();
                var token = await credential.GetTokenAsync(new TokenRequestContext(new[] { AzureSqlScope }));
                accessToken = token.Token;
            }

            sqlConnection.AccessToken = accessToken;
            await sqlConnection.OpenAsync();
            return sqlConnection;
        }

        private static bool ContainsSqlUserPassword(string connectionString) =>
            connectionString.IndexOf("User ID=", StringComparison.OrdinalIgnoreCase) >= 0
            || connectionString.IndexOf("UID=", StringComparison.OrdinalIgnoreCase) >= 0
            || connectionString.IndexOf("Password=", StringComparison.OrdinalIgnoreCase) >= 0
            || connectionString.IndexOf("PWD=", StringComparison.OrdinalIgnoreCase) >= 0;

        private static void VerifyConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(
                    $"{nameof(DbConnection)}.{nameof(CreateSqlConnection)} was called without a connection string for the Employment Check Database.");
            }
        }
    }
}
