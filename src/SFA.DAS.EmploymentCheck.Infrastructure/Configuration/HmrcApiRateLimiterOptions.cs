using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Configuration
{
    public class HmrcApiRateLimiterOptions : TableEntity
    {
        public int EmploymentCheckBatchSize { get; set; } = 1;
        public int DelayInMs { get; set; } = 0;
        public int DelayAdjustmentIntervalInMs { get; set; } = 50;
        public int MinimumReduceDelayIntervalInMinutes { get; set; } = 7*24*60;
        public int MinimumIncreaseDelayIntervalInSeconds { get; set; } = 1;
        public int TooManyRequestsRetryCount { get; set; } = 10;
        public int TransientErrorRetryCount { get; set; } = 3;
        public int TransientErrorDelayInMs { get; set; } = 10000;
        public int TokenRetrievalRetryCount { get; set; } = 2;
        public int TokenFailureRetryDelayInMs { get; set; } = 1000;
        public DateTime UpdateDateTime { get; set; }
    }
}