using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    public class ApiOptionsRepository : IApiOptionsRepository
    {
        //private const string RowKey = "ApiRetryOptions";
        private const string StorageTableName = "EmploymentCheckApiOptions";
        private readonly AzureStorageConnectionConfiguration _apiConnectionConfiguration;
        private CloudTable _table;

        public ApiOptionsRepository(AzureStorageConnectionConfiguration options)
        {
            _apiConnectionConfiguration = options;
        }

        private void InitTableStorage()
        {
            var storageAccount = CloudStorageAccount.Parse(_apiConnectionConfiguration.StorageAccountConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference(StorageTableName);
            _table.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        private CloudTable GetTable()
        {
            if (_table == null) InitTableStorage();
            return _table;
        }

        public async Task<ApiRetryOptions> GetOptions(string rowKey = "ApiRetryOptions")
        {
            var query = new TableQuery<ApiRetryOptions>();
            var queryResult = await GetTable().ExecuteQuerySegmentedAsync(query, null);
            var record = queryResult.Results.SingleOrDefault(
                r => r.RowKey == rowKey && r.PartitionKey == _apiConnectionConfiguration.EnvironmentName);

            return record ?? GetDefaultOptions(rowKey);
        }

        private ApiRetryOptions GetDefaultOptions(string rowKey)
        {
            return new ApiRetryOptions
            {
                RowKey = rowKey,
                PartitionKey = _apiConnectionConfiguration.EnvironmentName,
            };
        }

    }
}
