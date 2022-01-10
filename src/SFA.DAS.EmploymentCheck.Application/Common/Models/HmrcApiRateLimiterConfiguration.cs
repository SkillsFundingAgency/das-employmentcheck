namespace SFA.DAS.EmploymentCheck.Application.Common.Models
{
    public class HmrcApiRateLimiterConfiguration
    {
        public string StorageAccountConnectionString { get; set; } = "UseDevelopmentStorage=true";

        public string EnvironmentName { get; set; } = "LOCAL";
    }
}