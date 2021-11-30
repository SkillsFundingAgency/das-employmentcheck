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


        public async Task ReduceDelaySetting(int value)
        {
            var record = await GetHmrcRateLimiterOptions();

            //var query =  TableOperation.Retrieve<HmrcApiRateLimiterOptions>
            //    (_rateLimiterConfiguration.EnvironmentName, RowKey);

            //var retrieveOperation = new TableQuery<HmrcApiRateLimiterOptions>();
            //TableOperation.Retrieve<HmrcApiRateLimiterOptions>("Skype", "skypeid");

            //var record = await _table.ExecuteAsync<HmrcApiRateLimiterOptions>(TableOperation.Retrieve("",""));

           // var record = await _table.ExecuteQuerySegmentedAsync(retrieveOperation, null).Result;


            //var record = _table.ExecuteAsync(new TableQuery<HmrcApiRateLimiterOptions>())
            //    .SingleOrDefault(x => x.RowKey == RowKey) ?? GetDefaultOptions();


            var timeSinceLastUpdate = DateTime.UtcNow - record.UpdateDateTime;
            if (timeSinceLastUpdate < TimeSpan.FromDays(record.MinimumUpdatePeriodInDays)) return;

            record.DelayInMs = value;
            record.UpdateDateTime = DateTime.UtcNow;
            record.PartitionKey = _rateLimiterConfiguration.EnvironmentName;

            var operation = TableOperation.InsertOrReplace(record);
            await _table.ExecuteAsync(operation);
        }

        public async Task<HmrcApiRateLimiterOptions> GetHmrcRateLimiterOptions()
        {
            var query = new TableQuery<HmrcApiRateLimiterOptions>();
            var queryResult = await _table.ExecuteQuerySegmentedAsync(query, null);
            var record = queryResult.Results.SingleOrDefault(
                r => r.RowKey == RowKey && r.PartitionKey == _rateLimiterConfiguration.EnvironmentName);
            
            return record ?? GetDefaultOptions();
        }

        public async Task IncreaseDelaySetting(int value)
        {
            var record = await GetHmrcRateLimiterOptions() ?? GetDefaultOptions();
            record.DelayInMs = value;
            record.UpdateDateTime = DateTime.UtcNow;
            record.PartitionKey = _rateLimiterConfiguration.EnvironmentName;

            var operation = TableOperation.InsertOrReplace(record);
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