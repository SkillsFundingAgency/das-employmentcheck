using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Configuration;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.SubmitLearnerData
{
    public class SubmitLearnerDataApiClient
        : ISubmitLearnerDataApiClient
    {
        private HttpClient _httpClient;
        private IWebHostEnvironment _hostingEnvironment;
        private SubmitLearnerDataApiConfiguration _configuration;
        private IAzureClientCredentialHelper _azureClientCredentialHelper;
        private ILogger<IEmployerAccountApiClient> _logger;

        public SubmitLearnerDataApiClient(
            IHttpClientFactory httpClientFactory,
            SubmitLearnerDataApiConfiguration apiConfiguration,
            IWebHostEnvironment hostingEnvironment,
            IAzureClientCredentialHelper azureClientCredentialHelper,
            ILogger<IEmployerAccountApiClient> logger)
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
            var thisMethodName = "SubmitLearnerDataApiClient.Get()";

            string json = string.Empty;

            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                await AddAuthenticationHeader(httpRequestMessage);

                var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return JsonConvert.DeserializeObject<TResponse>(json);
        }

        private async Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
        {
            var thisMethodName = "SubmitLearnerDataApiClient.AddAuthenticationHeader()";

            try
            {
                if (!_hostingEnvironment.IsDevelopment() && !_httpClient.BaseAddress.IsLoopback)
                {
                    var accessToken = await _azureClientCredentialHelper.GetAccessTokenAsync(_configuration.Identifier);
                    httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}
