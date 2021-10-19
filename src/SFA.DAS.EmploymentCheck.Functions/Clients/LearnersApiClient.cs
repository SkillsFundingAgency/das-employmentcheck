using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmploymentCheck.Functions.Configuration;

namespace SFA.DAS.EmploymentCheck.Functions.Clients
{
    public class LearnersApiClient : ILearnersApiClient
    {
        private HttpClient _httpClient;
        private IWebHostEnvironment _hostingEnvironment;
        private LearnersApiConfiguration _configuration;
        private IAzureClientCredentialHelper _azureClientCredentialHelper;
        private ILogger<IAccountsApiClient> _logger;

        public LearnersApiClient(
            IHttpClientFactory httpClientFactory,
            LearnersApiConfiguration apiConfiguration,
            IWebHostEnvironment hostingEnvironment,
            IAzureClientCredentialHelper azureClientCredentialHelper,
            ILogger<IAccountsApiClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(apiConfiguration.Url);
            _hostingEnvironment = hostingEnvironment;
            _configuration = apiConfiguration;
            _azureClientCredentialHelper = azureClientCredentialHelper;
            _logger = logger;
        }


        public async Task<TResponse> Get<TResponse>(string url)
        {
            var thisMethodName = "Client: LearnersApiClient.Get()";
            var messagePrefix = $"\n\n{ DateTime.UtcNow } UTC { thisMethodName}:";

            //_logger.LogInformation($"{messagePrefix} Started.");

            string json = string.Empty;

            try
            {
                //_logger.LogInformation($"{messagePrefix} Executing Http Get Request to {url}.");
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                await AddAuthenticationHeader(httpRequestMessage);

                var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                //_logger.LogInformation($"{messagePrefix} Http Get Request returned {json}.");
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            //_logger.LogInformation($"{messagePrefix} Completed.");
            return JsonConvert.DeserializeObject<TResponse>(json);
        }

        private async Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
        {
            if (!_hostingEnvironment.IsDevelopment() && !_httpClient.BaseAddress.IsLoopback)
            {
                var accessToken = await _azureClientCredentialHelper.GetAccessTokenAsync(_configuration.Identifier);
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }
    }
}
