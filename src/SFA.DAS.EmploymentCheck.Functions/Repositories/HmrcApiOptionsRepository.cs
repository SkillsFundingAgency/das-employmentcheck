using Microsoft.Azure.Cosmos.Table;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using System;
using System.Linq;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public class HmrcApiOptionsRepository : IHmrcApiOptionsRepository
    {
        private const string RowKey = "HmrcApiRateLimiterOptions";
        private const string StorageTableName = "EmploymentCheckHmrcApiRateLimiterOptions";

        private const int DefaultDelayInMs = 110;
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
            _table.CreateIfNotExists();
        }

        public void ReduceDelaySetting(int value)
        {
            var record = _table.ExecuteQuery(new TableQuery<HmrcApiRateLimiterOptions>())
                .SingleOrDefault(x => x.RowKey == RowKey) ?? GetDefaultOptions();

            var timeSinceLastUpdate = DateTime.UtcNow - record.UpdateDateTime;
            if (timeSinceLastUpdate < TimeSpan.FromDays(record.MinimumUpdatePeriodInDays)) return;

            record.DelayInMs = value;
            record.UpdateDateTime = DateTime.UtcNow;
            record.PartitionKey = _rateLimiterConfiguration.EnvironmentName;

            var operation = TableOperation.InsertOrReplace(record);
            _table.Execute(operation);
        }

        public void IncreaseDelaySetting(int value)
        {
            var record = _table.ExecuteQuery(new TableQuery<HmrcApiRateLimiterOptions>())
                .SingleOrDefault(x => x.RowKey == RowKey) ?? GetDefaultOptions();

            record.DelayInMs = value;
            record.UpdateDateTime = DateTime.UtcNow;
            record.PartitionKey = _rateLimiterConfiguration.EnvironmentName;

            var operation = TableOperation.InsertOrReplace(record);
            _table.Execute(operation);
        }

        public HmrcApiRateLimiterOptions GetHmrcRateLimiterOptions()
        {
            var record = _table.ExecuteQuery(new TableQuery<HmrcApiRateLimiterOptions>())
                .SingleOrDefault(x => x.RowKey == RowKey);

            return record ?? GetDefaultOptions();
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