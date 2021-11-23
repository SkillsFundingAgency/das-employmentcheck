using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using SFA.DAS.EmploymentCheck.Functions.Configuration;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public class HmrcApiOptionsRepository : IHmrcApiOptionsRepository
    {
        private const int DefaultDelayInMs = 1000; // 1 second
        private readonly HmrcApiRateLimiterConfiguration _rateLimiterConfiguration;
        private CloudTable _table;

        public HmrcApiOptionsRepository(IOptions<HmrcApiRateLimiterConfiguration> options)
        {
            _rateLimiterConfiguration = options.Value;
            InitTableStorage();
        }

        private void InitTableStorage()
        {
            var storageAccount = CloudStorageAccount.Parse(_rateLimiterConfiguration.StorageAccountConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference(_rateLimiterConfiguration.StorageTableName);
            _table.CreateIfNotExists();
        }

        public int GetRequestDelayInMsSetting()
        {
            var record = _table.ExecuteQuery(new TableQuery<HmrcApiRateLimiterOptions>())
                .SingleOrDefault(x => x.RowKey == HmrcApiRateLimiterConfiguration.ConfigSection);
            
            return record?.DelayInMs ?? DefaultDelayInMs;
        }

        public void UpdateRequestDelaySetting(int value)
        {
            var record = _table.ExecuteQuery(new TableQuery<HmrcApiRateLimiterOptions>())
                .Single(x => x.RowKey == HmrcApiRateLimiterConfiguration.ConfigSection);

            var timeSinceLastUpdate = DateTime.UtcNow - record.UpdateDateTime;
            if (timeSinceLastUpdate < TimeSpan.FromDays(_rateLimiterConfiguration.MinimumUpdatePeriodInDays)) return;

            record.DelayInMs = value;
            record.UpdateDateTime = DateTime.UtcNow;
            record.PartitionKey = _rateLimiterConfiguration.EnvironmentName;

            var operation = TableOperation.InsertOrReplace(record);
            _table.Execute(operation);
        }

        public HmrcApiRateLimiterOptions GetHmrcRateLimiterOptions()
        {
            var record = _table.ExecuteQuery(new TableQuery<HmrcApiRateLimiterOptions>())
                .Single(x => x.RowKey == HmrcApiRateLimiterConfiguration.ConfigSection);

            return record;
        }


    }
}