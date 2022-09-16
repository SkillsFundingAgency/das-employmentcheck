using SFA.DAS.TokenService.Api.Client;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class TokenServiceApiClientConfiguration : ITokenServiceApiClientConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string IdentifierUri { get; set; }
        public string Tenant { get; set; }
    }

}
