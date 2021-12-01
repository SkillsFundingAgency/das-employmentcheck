using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.SubmitLearnerData
{
    public class DcTokenService : IDcTokenService
    {
        private readonly IHttpClientFactory _httpFactory;

        public DcTokenService(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<AuthResult> GetTokenAsync(string baseUrl, string grantType, string secret, string clientId, string scope)
        {
            using (HttpClient client = _httpFactory.CreateClient("TokenRequest"))
            {
                var form = new Dictionary<string, string>
                {
                    { "grant_type", grantType },
                    { "client_id", clientId },
                    { "client_secret", secret },
                    { "scope", scope }
                };

                try
                {
                    HttpResponseMessage tokenResponse = await client.PostAsync(baseUrl, new FormUrlEncodedContent(form));
                    var jsonContent = await tokenResponse.Content.ReadAsStreamAsync();
                    AuthResult token = await JsonSerializer.DeserializeAsync<AuthResult>(jsonContent);
                    return token;
                }
                catch (Exception ex)
                {
                    return new AuthResult();
                }
            }
        }
    }
}