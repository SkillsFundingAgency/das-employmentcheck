namespace SFA.DAS.EmploymentCheck.Infrastructure.Configuration
{
    public class DcApiSettings
    {
        public string BaseUrl { get; set; }
        public string Tenant { get; set; }
        public string IdentifierUri { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}