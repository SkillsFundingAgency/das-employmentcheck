
namespace SFA.DAS.EmploymentCheck.Infrastructure.Configuration
{
    public class HmrcApiRateLimiterOptions 
    {
        public int EmploymentCheckBatchSize { get; set; } 
        public int DelayAdjustmentIntervalInMs { get; set; } 
        public int MinimumReduceDelayIntervalInMinutes { get; set; } 
        public int MinimumIncreaseDelayIntervalInSeconds { get; set; }
        public int TokenFailureRetryDelayInMs { get; set; }
    }
}