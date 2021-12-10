using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public class HmrcApiOptionsRepository : IHmrcApiOptionsRepository
    {
        private const string RowKey = "HmrcApiRateLimiterOptions";
        private const string StorageTableName = "EmploymentCheckHmrcApiRateLimiterOptions";

        private const int DefaultDelayInMs = 1000;
        private const int DefaultDelayAdjustmentIntervalInMs = 100;
        private readonly HmrcApiRateLimiterConfiguration _rateLimiterConfiguration;
        private CloudTable _table;

        public HmrcApiOptionsRepository(HmrcApiRateLimiterConfiguration options)
        {
            _rateLimiterConfiguration = options;
            InitTableStorage();
        }

        private void InitTableStorage()
        {
            var storageAccount = CloudStorageAccount.Parse(_rateLimiterConfiguration.StorageAccountConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference(StorageTableName);
            _table.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        public async Task<HmrcApiRateLimiterOptions> GetHmrcRateLimiterOptions()
        {
            var query = new TableQuery<HmrcApiRateLimiterOptions>();
            var queryResult = await _table.ExecuteQuerySegmentedAsync(query, null);
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

            var operation = TableOperation.InsertOrReplace(options);
            await _table.ExecuteAsync(operation);
        }

        public async Task IncreaseDelaySetting(HmrcApiRateLimiterOptions options)
        {
            options.DelayInMs += options.DelayAdjustmentIntervalInMs;
            options.UpdateDateTime = DateTime.UtcNow;

            var operation = TableOperation.InsertOrReplace(options);
            await _table.ExecuteAsync(operation);
        }

        private HmrcApiRateLimiterOptions GetDefaultOptions()
        {
            return new HmrcApiRateLimiterOptions
            {
                RowKey = RowKey,
                PartitionKey = _rateLimiterConfiguration.EnvironmentName,
                DelayInMs = DefaultDelayInMs,
                DelayAdjustmentIntervalInMs = DefaultDelayAdjustmentIntervalInMs,
                MinimumUpdatePeriodInDays = 0
            };
        }
    }
}