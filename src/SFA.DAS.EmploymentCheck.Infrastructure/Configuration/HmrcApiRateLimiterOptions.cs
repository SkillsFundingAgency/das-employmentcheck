using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Configuration
{
    public class HmrcApiRateLimiterOptions : TableEntity
    {
        public int DelayInMs { get; set; }
        public int DelayAdjustmentIntervalInMs { get; set; }
        public int MinimumUpdatePeriodInDays { get; set; }
        public DateTime UpdateDateTime { get; set; }
    }
}