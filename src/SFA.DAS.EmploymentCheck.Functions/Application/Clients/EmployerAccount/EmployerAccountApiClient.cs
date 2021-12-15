using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount
{
    public class EmployerAccountApiClient : IEmployerAccountApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly EmployerAccountApiConfiguration _configuration;
        private readonly IAzureClientCredentialHelper _azureClientCredentialHelper;
        private readonly ILogger<IEmployerAccountApiClient> _logger;

        public EmployerAccountApiClient(
            IHttpClientFactory httpClientFactory,
            EmployerAccountApiConfiguration apiConfiguration,
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
            const string thisMethodName = "EmployerAccountApiClient.Get()";

            var json = string.Empty;
            HttpResponseMessage response = null;

            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                await AddAuthenticationHeader(httpRequestMessage);

                response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {response?.Content.ReadAsStringAsync()} {ex.StackTrace}");
                return JsonConvert.DeserializeObject<TResponse>("");
            }

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
