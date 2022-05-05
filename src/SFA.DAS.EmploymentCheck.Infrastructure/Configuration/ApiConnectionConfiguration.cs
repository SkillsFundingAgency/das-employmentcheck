namespace SFA.DAS.EmploymentCheck.Infrastructure.Configuration
{
    public class ApiConnectionConfiguration
    {
        public string StorageAccountConnectionString { get; set; } = "UseDevelopmentStorage=true";
        public string EnvironmentName { get; set; } = "LOCAL";
    }
}