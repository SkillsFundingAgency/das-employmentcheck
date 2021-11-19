using Newtonsoft.Json;

namespace SFA.DAS.EmploymentCheck.TokenServiceStub.Services
{
    public class OAuthTokenRequest
    {
        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("grant_type")]
        public string GrantType { get; set; }

        [JsonProperty("scopes")]
        public string Scopes { get; set; }
    }
}