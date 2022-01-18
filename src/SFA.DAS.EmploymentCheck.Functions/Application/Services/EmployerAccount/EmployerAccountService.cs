using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using SFA.DAS.HashingService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount
{
    public class EmployerAccountService
        : IEmployerAccountService
    {
        #region Private memebers
        private readonly ILogger<EmployerAccountService> _logger;
        private readonly IHashingService _hashingService;
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly EmployerAccountApiConfiguration _configuration;
        private readonly IAzureClientCredentialHelper _azureClientCredentialHelper;
        private readonly IAccountsResponseRepository _repository;
        #endregion Private memebers

        #region Constructors
        public EmployerAccountService(
            ILogger<EmployerAccountService> logger,
            EmployerAccountApiConfiguration apiConfiguration,
            IHashingService hashingService,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment hostingEnvironment,
            IAzureClientCredentialHelper azureClientCredentialHelper,
            IAccountsResponseRepository repository
        )
        {
            _logger = logger;
            _configuration = apiConfiguration;
            _hashingService = hashingService;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(apiConfiguration.Url);
            _hostingEnvironment = hostingEnvironment;
            _azureClientCredentialHelper = azureClientCredentialHelper;
            _repository = repository;
        }
        #endregion Constructors

        #region GetEmployerPayeSchemes
        public async Task<EmployerPayeSchemes> GetEmployerPayeSchemes(Models.EmploymentCheck employmentChecksBatch)
        {
            Guard.Against.Null(employmentChecksBatch, nameof(employmentChecksBatch));

            var resourceList = await Get<ResourceList>(employmentChecksBatch);
            if (resourceList.GetType() != typeof(ResourceList))
            {
                // no PayeScheme found, return an empty EmployerPayeScheme (caller should check for empty EmployerPayeScheme
                return new EmployerPayeSchemes(employmentChecksBatch.AccountId, new List<string>());
            }

            return new EmployerPayeSchemes(employmentChecksBatch.AccountId, resourceList.Select(x => x.Id).ToList());
        }

        public async Task<TResponse> Get<TResponse>(Models.EmploymentCheck employmentCheckBatch)
        {
            var hashedAccountId = _hashingService.HashValue(employmentCheckBatch.AccountId);
            var url = $"api/accounts/{hashedAccountId}/payeschemes";

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            await AddAuthenticationHeader(httpRequestMessage);

            // setup a default template 'response' to store the api response
            var accountsResponse = new AccountsResponse(
                employmentCheckBatch.Id,
                employmentCheckBatch.CorrelationId,
                employmentCheckBatch.AccountId,
                string.Empty,   // PayeSchemes,
                string.Empty,   // Response
                -1);            // Http Status Code

            // Call the Accounts API
            TResponse content;
            try
            {
                var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                content = JsonConvert.DeserializeObject<TResponse>(json);
                var resourceList = new ResourceList((IEnumerable<ResourceViewModel>)await Task.FromResult(content));
                var employerPayeSchemes = new EmployerPayeSchemes(employmentCheckBatch.AccountId, resourceList.Select(x => x.Id).ToList());
                var allEmployerPayeSchemes = new StringBuilder();
                foreach (var payeScheme in employerPayeSchemes.PayeSchemes)
                {
                    allEmployerPayeSchemes.Append($", {payeScheme}");
                }

                // trim the comma at start of string
                var responsePayeSchemes = allEmployerPayeSchemes.ToString();
                responsePayeSchemes = responsePayeSchemes.Remove(0, 1);

                accountsResponse.PayeSchemes = responsePayeSchemes;
                accountsResponse.HttpResponse = response.ToString();
                accountsResponse.HttpStatusCode = (short)response.StatusCode;
            }
            catch (Exception e)
            {
                accountsResponse.HttpResponse = e.Message;
                content = (TResponse)new object();
                _logger.LogError($"EmployerAccountService.Get(): ERROR: {accountsResponse.HttpResponse}");
            }
            finally
            {
                try
                {
                    await _repository.Save(accountsResponse);
                }
                catch (Exception e)
                {
                    _logger.LogError($"EmployerAccountService.Get(): ERROR: the AccountsResponseRepository Save() method threw an Exception [{e}]");
                }
            }

            return content;
        }

        private async Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
        {
            if (!_hostingEnvironment.IsDevelopment() && !_httpClient.BaseAddress.IsLoopback)
            {
                var accessToken = await _azureClientCredentialHelper.GetAccessTokenAsync(_configuration.Identifier);
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }
        #endregion GetEmployerPayeSchemes
    }
}