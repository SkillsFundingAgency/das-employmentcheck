using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.ApiClients;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Services.Learner
{
    public class DataCollectionsApiClient : GetApiClient<DataCollectionsApiConfiguration>, IDataCollectionsApiClient<DataCollectionsApiConfiguration>
    {
        private readonly DataCollectionsApiConfiguration _configuration;
        private readonly IDcTokenService _tokenService;
        private readonly ILogger<DataCollectionsApiClient> _logger;

        public DataCollectionsApiClient(
            IHttpClientFactory httpClientFactory,
            DataCollectionsApiConfiguration configuration,
            IWebHostEnvironment hostingEnvironment,
            IDcTokenService tokenService,
            ILogger<DataCollectionsApiClient> logger) : base(httpClientFactory, configuration, hostingEnvironment)
        {
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        protected override async Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
        {
            await RetrieveAuthenticationToken(httpRequestMessage);
        }

        private async Task RetrieveAuthenticationToken(HttpRequestMessage httpRequestMessage)
        {
            _logger.LogInformation("{Client}: Getting DC access token…", nameof(DataCollectionsApiClient));

            var auth = await GetDataCollectionsApiAccessToken();

            if (auth == null || string.IsNullOrWhiteSpace(auth.AccessToken))
            {
                _logger.LogError("{Client}: Failed to acquire DC access token (null/empty).", nameof(DataCollectionsApiClient));
                throw new HttpRequestException("Failed to acquire DC access token.");
            }

            if (!HostingEnvironment.IsDevelopment() && !HttpClient.BaseAddress.IsLoopback)
            {
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
            }
        }

        private async Task<AuthResult> GetDataCollectionsApiAccessToken()
        {
            var authority = NormalizeAuthority(_configuration.Tenant);

            var result = await _tokenService.GetTokenAsync(
                authority,
                "client_credentials",
                _configuration.ClientSecret,
                _configuration.ClientId,
                _configuration.IdentifierUri);

            return result;
        }

        private static string NormalizeAuthority(string tenantValue)
        {
            if (string.IsNullOrWhiteSpace(tenantValue))
                throw new InvalidOperationException("DCLearnerDataApiTenant is required.");

            var v = tenantValue.Trim();


            if (Uri.TryCreate(v, UriKind.Absolute, out var abs))
            {
                var path = abs.AbsolutePath.TrimEnd('/');
                if (path.EndsWith("/oauth2/token", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith("/oauth2/v2.0/token", StringComparison.OrdinalIgnoreCase))
                {
                    return abs.ToString();
                }
                
                return new Uri(new Uri($"{abs.Scheme}://{abs.Authority}"),
                               $"{path.TrimEnd('/')}/oauth2/v2.0/token").ToString();
            }

            v = v.TrimStart('/');

            if (v.Contains("oauth2", StringComparison.OrdinalIgnoreCase))
            {
                var candidate = $"https://login.microsoftonline.com/{v}";
                if (candidate.EndsWith("/oauth2/token", StringComparison.OrdinalIgnoreCase) ||
                    candidate.EndsWith("/oauth2/v2.0/token", StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
                return candidate.TrimEnd('/') + "/oauth2/v2.0/token";
            }

            return $"https://login.microsoftonline.com/{v.TrimEnd('/')}/oauth2/v2.0/token";
        }
    }
}
