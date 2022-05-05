using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    public class ApiOptionsRepository : IApiOptionsRepository
    {
        private const string RowKey = "ApiRetryOptions";
        private const string StorageTableName = "EmploymentCheckApiOptions";
        private readonly ApiConnectionConfiguration _apiConnectionConfiguration;
        private readonly ILogger<ApiOptionsRepository> _logger;
        private CloudTable _table;

        public ApiOptionsRepository(ApiConnectionConfiguration options, ILogger<ApiOptionsRepository> logger)
        {
            _apiConnectionConfiguration = options;
            _logger = logger;
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

        public async Task<ApiRetryOptions> GetOptions()
        {
            var query = new TableQuery<ApiRetryOptions>();
            var queryResult = await GetTable().ExecuteQuerySegmentedAsync(query, null);
            var record = queryResult.Results.SingleOrDefault(
                r => r.RowKey == RowKey && r.PartitionKey == _apiConnectionConfiguration.EnvironmentName);

            return record ?? GetDefaultOptions();
        }

        public async Task IncreaseDelaySetting(ApiRetryOptions options)
        {
            options.DelayInMs += options.DelayAdjustmentIntervalInMs;
            options.UpdateDateTime = DateTime.UtcNow;

            _logger.LogInformation("[HmrcApiOptionsRepository] Increasing DelayInMs setting to {0}ms", new { options.DelayInMs });

            var operation = TableOperation.InsertOrReplace(options);
            await GetTable().ExecuteAsync(operation);
        }

        public async Task ReduceDelaySetting(ApiRetryOptions options)
        {
            var timeSinceLastUpdate = DateTime.UtcNow - options.UpdateDateTime;
            if (timeSinceLastUpdate < TimeSpan.FromDays(options.MinimumUpdatePeriodInDays)) return;

            options.DelayInMs = Math.Max(0, options.DelayInMs - options.DelayAdjustmentIntervalInMs);
            options.UpdateDateTime = DateTime.UtcNow;

            _logger.LogInformation("[HmrcApiOptionsRepository] Reducing DelayInMs setting to {0}ms", new { options.DelayInMs });

            var operation = TableOperation.InsertOrReplace(options);
            await GetTable().ExecuteAsync(operation);
        }

        private ApiRetryOptions GetDefaultOptions()
        {
            return new ApiRetryOptions
            {
                RowKey = RowKey,
                PartitionKey = _apiConnectionConfiguration.EnvironmentName,
            };
        }

    }
}
