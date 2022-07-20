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
    public class HmrcApiOptionsRepository : IHmrcApiOptionsRepository
    {
        private const string RowKey = "HmrcApiRateLimiterOptions";
        private const string StorageTableName = "EmploymentCheckHmrcApiRateLimiterOptions";
        private readonly AzureStorageConnectionConfiguration _rateLimiterConfiguration;
        private readonly ILogger<HmrcApiOptionsRepository> _logger;
        private CloudTable _table;

        public HmrcApiOptionsRepository(AzureStorageConnectionConfiguration options, ILogger<HmrcApiOptionsRepository> logger)
        {
            _rateLimiterConfiguration = options;
            _logger = logger;
        }

        private void InitTableStorage()
        {
            var storageAccount = CloudStorageAccount.Parse(_rateLimiterConfiguration.StorageAccountConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference(StorageTableName);
            _table.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        private CloudTable GetTable()
        {
            if (_table == null) InitTableStorage();
            return _table;
        }

        public async Task<HmrcApiRateLimiterOptions> GetHmrcRateLimiterOptions()
        {
            var query = new TableQuery<HmrcApiRateLimiterOptions>();
            var queryResult = await GetTable().ExecuteQuerySegmentedAsync(query, null);
            var record = queryResult.Results.SingleOrDefault(
                r => r.RowKey == RowKey && r.PartitionKey == _rateLimiterConfiguration.EnvironmentName);

            return record ?? GetDefaultOptions();
        }

        public async Task ReduceDelaySetting(HmrcApiRateLimiterOptions options)
        {
            var timeSinceLastUpdate = DateTime.UtcNow - options.UpdateDateTime;
            if (timeSinceLastUpdate < TimeSpan.FromDays(options.MinimumUpdatePeriodInDays)) return;

            options.DelayInMs = Math.Max(0, options.DelayInMs - options.DelayAdjustmentIntervalInMs);
            options.UpdateDateTime = DateTime.UtcNow;

            _logger.LogInformation("[HmrcApiOptionsRepository] Reducing DelayInMs setting to {0}ms", new { options.DelayInMs });

            var operation = TableOperation.InsertOrReplace(options);
            await GetTable().ExecuteAsync(operation);
        }

        public async Task IncreaseDelaySetting(HmrcApiRateLimiterOptions options)
        {
            options.DelayInMs += options.DelayAdjustmentIntervalInMs;
            options.UpdateDateTime = DateTime.UtcNow;

            _logger.LogInformation("[HmrcApiOptionsRepository] Increasing DelayInMs setting to {0}ms", new { options.DelayInMs });

            var operation = TableOperation.InsertOrReplace(options);
            await GetTable().ExecuteAsync(operation);
        }

        private HmrcApiRateLimiterOptions GetDefaultOptions()
        {
            return new HmrcApiRateLimiterOptions
            {
                RowKey = RowKey,
                PartitionKey = _rateLimiterConfiguration.EnvironmentName,
            };
        }
    }
}