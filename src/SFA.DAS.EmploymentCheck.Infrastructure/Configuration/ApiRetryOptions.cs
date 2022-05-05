using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Configuration
{
    public class ApiRetryOptions : TableEntity
    {
       
        public int MinimumUpdatePeriodInDays { get; set; } = 7;
        public int TooManyRequestsRetryCount { get; set; } = 10;
        public int TransientErrorRetryCount { get; set; } = 3;
        public int TransientErrorDelayInMs { get; set; } = 10000;
        public int TokenRetrievalRetryCount { get; set; } = 2;
        public int TokenFailureRetryDelayInMs { get; set; } = 1000;
        public DateTime UpdateDateTime { get; set; }
    }
}
