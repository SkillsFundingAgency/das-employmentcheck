using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.ApiClients;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
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
            _logger.LogInformation($"{nameof(DataCollectionsApiClient)}: Getting access token...");
            var accessToken = await GetDataCollectionsApiAccessToken();
            if (!HostingEnvironment.IsDevelopment() && !HttpClient.BaseAddress.IsLoopback)
            {
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);
            }
        }

        private async Task<AuthResult> GetDataCollectionsApiAccessToken()
        {
            var result = await _tokenService.GetTokenAsync(
                $"https://login.microsoftonline.com/{_configuration.Tenant}",
                "client_credentials",
                _configuration.ClientSecret,
                _configuration.ClientId,
                _configuration.IdentifierUri);

            return result;
        }
    }
}
