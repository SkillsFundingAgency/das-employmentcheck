using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public class EmploymentCheckCacheRequestRepository : IEmploymentCheckCacheRequestRepository
    {
        private readonly string _connectionString;
        private const string AzureResource = "https://database.windows.net/";

        public EmploymentCheckCacheRequestRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> Insert<T>(T entity) where T : class
        {
            await using var dbConnection = GetSqlConnection(_connectionString);
            return await dbConnection.InsertAsync(entity);
        }

        private static SqlConnection GetSqlConnection(string connectionString)
        {
            return new SqlConnection() { ConnectionString = connectionString, AccessToken = connectionString.Contains("Integrated Security") ? null : new AzureServiceTokenProvider().GetAccessTokenAsync(AzureResource).Result };
        }
    }
}