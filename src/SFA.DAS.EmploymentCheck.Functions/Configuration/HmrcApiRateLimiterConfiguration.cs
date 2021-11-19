namespace SFA.DAS.EmploymentCheck.Functions.Configuration
{
    public class HmrcApiRateLimiterConfiguration
    {
        public const string ConfigSection = "HmrcApiRateLimiterConfiguration";
        public int MinimumUpdatePeriodInDays { get; set; } = 7;
        public string StorageAccountConnectionString { get; set; } = "UseDevelopmentStorage=true";
        public string EnvironmentName { get; set; } = "DEV";
        public string StorageTableName { get; set; } = "HmrcApiRateLimiterOptions";
    }
}