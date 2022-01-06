namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    public class DcApiSettings
    {
        public string BaseUrl { get; set; }
        public int PageSize { get; set; }
        public int TaskSize { get; set; }
        public string Tenant { get; set; }
        public string IdentifierUri { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}