using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmploymentCheck.Functions.Configuration;

namespace SFA.DAS.EmploymentCheck.Functions.Clients
{
    public class AccountsApiClient : IAccountsApiClient
    {
        private HttpClient _httpClient;
        private IWebHostEnvironment _hostingEnvironment;
        private AccountsApiConfiguration _configuration;
        private IAzureClientCredentialHelper _azureClientCredentialHelper;

        public AccountsApiClient(
            IHttpClientFactory httpClientFactory,
            AccountsApiConfiguration apiConfiguration,
            IWebHostEnvironment hostingEnvironment,
            IAzureClientCredentialHelper azureClientCredentialHelper)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(apiConfiguration.Url);
            _hostingEnvironment = hostingEnvironment;
            _configuration = apiConfiguration;
            _azureClientCredentialHelper = azureClientCredentialHelper;
        }


        public async Task<TResponse> Get<TResponse>(string url)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            await AddAuthenticationHeader(httpRequestMessage);

            var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<TResponse>(json);
        }

        private async Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
        {
            if (!_hostingEnvironment.IsDevelopment())
            {
                var accessToken = await _azureClientCredentialHelper.GetAccessTokenAsync(_configuration.Identifier);
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }
    }
}
