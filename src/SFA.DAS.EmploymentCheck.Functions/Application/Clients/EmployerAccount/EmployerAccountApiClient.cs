﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmploymentCheck.Functions.Configuration;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount
{
    public class EmployerAccountApiClient : IEmployerAccountApiClient
    {
        private HttpClient _httpClient;
        private IWebHostEnvironment _hostingEnvironment;
        private EmployerAccountApiConfiguration _configuration;
        private IAzureClientCredentialHelper _azureClientCredentialHelper;
        private ILogger<IEmployerAccountApiClient> _logger;

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
            var thisMethodName = "EmployerAccountApiClient.Get()";
            string json = string.Empty;

            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                await AddAuthenticationHeader(httpRequestMessage);

                var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
                json = string.Empty;
            }

            return JsonConvert.DeserializeObject<TResponse>(json);
        }

        private async Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
        {
            var thisMethodName = "EmployerAccountApiClient.AddAuthenticationHeader()";

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
