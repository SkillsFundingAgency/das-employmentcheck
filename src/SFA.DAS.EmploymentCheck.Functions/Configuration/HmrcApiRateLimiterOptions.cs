using System;
using Microsoft.Azure.Cosmos.Table;

namespace SFA.DAS.EmploymentCheck.Functions.Configuration
{
    public class HmrcApiRateLimiterOptions : TableEntity
    {
        public int BatchSize { get; set; }
        public int DelayInMs { get; set; }
        public int DelayAdjustmentIntervalInMs { get; set; }
        public DateTime UpdateDateTime { get; set; }
    }
}