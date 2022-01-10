using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace SFA.DAS.EmploymentCheck.Application.Common.Models
{
    public class HmrcApiRateLimiterOptions : TableEntity
    {
        public int DelayInMs { get; set; }

        public int DelayAdjustmentIntervalInMs { get; set; }

        public int MinimumUpdatePeriodInDays { get; set; }

        public DateTime UpdateDateTime { get; set; }
    }
}