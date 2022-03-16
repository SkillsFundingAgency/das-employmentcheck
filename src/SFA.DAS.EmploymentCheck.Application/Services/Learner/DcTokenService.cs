using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Services.Learner
{
    public class DcTokenService : IDcTokenService
    {
        private readonly IHttpClientFactory _httpFactory;

        public DcTokenService(
            IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<AuthResult> GetTokenAsync(string baseUrl, string grantType, string secret, string clientId,
            string scope)
        {
            using var client = _httpFactory.CreateClient("TokenRequest");
            
            var form = new Dictionary<string, string>
            {
                {"grant_type", grantType},
                {"client_id", clientId},
                {"client_secret", secret},
                {"scope", scope}
            };


            var tokenResponse = await client.PostAsync(baseUrl, new FormUrlEncodedContent(form));
            var jsonContent = await tokenResponse.Content.ReadAsStreamAsync();
            var token = await JsonSerializer.DeserializeAsync<AuthResult>(jsonContent);

            return token;
        }
    }
}