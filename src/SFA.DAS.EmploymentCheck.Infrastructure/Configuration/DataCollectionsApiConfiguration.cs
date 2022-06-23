namespace SFA.DAS.EmploymentCheck.Infrastructure.Configuration
{
    public class DataCollectionsApiConfiguration : IApiConfiguration
    {
        public string BaseUrl { get; set; }
        public string Tenant { get; set; }
        public string IdentifierUri { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Path { get; set; }
    }
}