namespace SFA.DAS.EmploymentCheck.Application.Common.Models
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