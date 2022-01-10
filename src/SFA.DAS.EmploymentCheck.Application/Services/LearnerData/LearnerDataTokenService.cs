using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Interfaces.LearnerData;
using SFA.DAS.EmploymentCheck.Domain.Common;
using SFA.DAS.EmploymentCheck.Application.Common.Models;

namespace SFA.DAS.EmploymentCheck.Application.Services.LearnerData
{
    public class LearnerDataTokenService : ILearnerDataTokenService
    {
        private const string ThisClassName = "\n\nDcTokenService";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<LearnerDataTokenService> _logger;

        public LearnerDataTokenService(
            ILogger<LearnerDataTokenService> logger,
            IHttpClientFactory httpFactory)
        {
            _logger = logger;
            _httpFactory = httpFactory;
        }

        public async Task<AuthResult> GetTokenAsync(string baseUrl, string grantType, string secret, string clientId, string scope)
        {
            string thisMethodName = $"{nameof(LearnerDataTokenService)}.GetApprenticeNiNumbers()";

            using (HttpClient client = _httpFactory.CreateClient("TokenRequest"))
            {
                var form = new Dictionary<string, string>
                {
                    { "grant_type", grantType },
                    { "client_id", clientId },
                    { "client_secret", secret },
                    { "scope", scope }
                };

                AuthResult token = null;
                try
                {
                    HttpResponseMessage tokenResponse = await client.PostAsync(baseUrl, new FormUrlEncodedContent(form));
                    var jsonContent = await tokenResponse.Content.ReadAsStreamAsync();
                    token = await JsonSerializer.DeserializeAsync<AuthResult>(jsonContent);

                }
                catch (Exception ex)
                {
                    _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");

                    token = new AuthResult();
                }

                return token;
            }
        }
    }
}