using SFA.DAS.Http.Configuration;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Configuration
{
    public class ApplicationSettings : IManagedIdentityClientConfiguration
    {
        public string DbConnectionString { get; set; }
        public string AllowedHashstringCharacters { get; set; }
        public string Hashstring { get; set; }
        public string ApiBaseUrl { get; set; }
        public string IdentifierUri { get; set; }
    }
}
