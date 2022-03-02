namespace SFA.DAS.EmploymentCheck.Functions.Configuration
{
    public class HmrcApiRateLimiterConfiguration
    {
        public string StorageAccountConnectionString { get; set; } = "UseDevelopmentStorage=true";
        public string EnvironmentName { get; set; } = "LOCAL";
    }
}