namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class DcOAuthSettings
    {
        public string TokenUrl { get; set; }
        public string GrantType { get; set; }
        public string Scope { get; set; }
        public string ClientId { get; set; }
        public string SecretId { get; set; }
        public string SecretValue { get; set; }
    }
}