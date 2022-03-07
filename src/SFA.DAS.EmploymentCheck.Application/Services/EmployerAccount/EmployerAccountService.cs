using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using SFA.DAS.HashingService;

namespace SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount
{
    public class EmployerAccountService
        : IEmployerAccountService
    {
        private readonly ILogger<IEmployerAccountService> _logger;
        private readonly IHashingService _hashingService;
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly EmployerAccountApiConfiguration _configuration;
        private readonly IAzureClientCredentialHelper _azureClientCredentialHelper;
        private readonly IAccountsResponseRepository _repository;

        public EmployerAccountService(
            ILogger<IEmployerAccountService> logger,
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

        public async Task<EmployerPayeSchemes> GetEmployerPayeSchemes(Data.Models.EmploymentCheck employmentCheck)
        {
            EmployerPayeSchemes payeSchemes = null;
            if (employmentCheck != null && employmentCheck.Id != 0)
            {
                HttpRequestMessage httpRequestMessage = await SetupAccountsApiConfig(employmentCheck);
                payeSchemes = await GetPayeSchemes(employmentCheck, httpRequestMessage).ConfigureAwait(false);
            }

            return payeSchemes ?? new EmployerPayeSchemes();
        }

        private async Task<HttpRequestMessage> SetupAccountsApiConfig(Data.Models.EmploymentCheck employmentCheck)
        {
            var hashedAccountId = _hashingService.HashValue(employmentCheck.AccountId);
            var url = $"api/accounts/{hashedAccountId}/payeschemes";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            await AddAuthenticationHeader(httpRequestMessage);
            return httpRequestMessage;
        }

        private async Task<EmployerPayeSchemes> GetPayeSchemes(Data.Models.EmploymentCheck employmentCheck, HttpRequestMessage httpRequestMessage)
        {
            EmployerPayeSchemes employerPayeSchemes = null;
            try
            {
                var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                employerPayeSchemes = await GetPayeSchemesFromApiResponse(employmentCheck, response);
                employerPayeSchemes.HttpStatusCode = response.StatusCode;
            }
            catch (Exception e)
            {
                await HandleException(employmentCheck, e);
            }

            return employerPayeSchemes ?? new EmployerPayeSchemes();
        }

        private async Task<EmployerPayeSchemes> GetPayeSchemesFromApiResponse(
            Data.Models.EmploymentCheck employmentCheck,
            HttpResponseMessage httpResponseMessage
        )
        {
            Guard.Against.Null(employmentCheck, nameof(employmentCheck));
            var accountsResponse = await InitialiseAccountResponseModel(employmentCheck);

            if (httpResponseMessage == null)
            {
                await Save(accountsResponse);
                return await Task.FromResult(new EmployerPayeSchemes());
            }

            accountsResponse.HttpResponse = httpResponseMessage.ToString();
            accountsResponse.HttpStatusCode = (short)httpResponseMessage.StatusCode;

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                await Save(accountsResponse);
                return await Task.FromResult(new EmployerPayeSchemes());
            }

            var jsonContent = await ReadResponseContent(httpResponseMessage, accountsResponse);
            var employerPayeSchemes = await DeserialiseContent(jsonContent, accountsResponse);

            return employerPayeSchemes ?? new EmployerPayeSchemes();
        }

        private async Task<AccountsResponse> InitialiseAccountResponseModel(Data.Models.EmploymentCheck employmentCheck)
        {
            return await Task.FromResult(new AccountsResponse(
                0,
                employmentCheck.Id,
                employmentCheck.CorrelationId,
                employmentCheck.AccountId,
                string.Empty,                               // PayeSchemes,
                string.Empty,                               // Response
                (short)HttpStatusCode.InternalServerError,  // Http Status Code
                DateTime.Now));
        }

        private async Task<string> ReadResponseContent(
            HttpResponseMessage httpResponseMessage,
            AccountsResponse accountsResponse
        )
        {
            Guard.Against.Null(accountsResponse, nameof(accountsResponse));
            Guard.Against.Null(httpResponseMessage, nameof(httpResponseMessage));

            var json = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(json))
            {
                await Save(accountsResponse);
                return string.Empty;
            }

            return json;
        }

        private async Task<EmployerPayeSchemes> DeserialiseContent(
            string jsonContent,
            AccountsResponse accountsResponse
        )
        {
            Guard.Against.Null(accountsResponse, nameof(accountsResponse));
            if (string.IsNullOrEmpty(jsonContent))
            {
                await Save(accountsResponse);
                return await Task.FromResult(new EmployerPayeSchemes());
            }


            var resourceList = JsonConvert.DeserializeObject<ResourceList>(jsonContent);
            if (resourceList == null || !resourceList.Any())
            {
                await Save(accountsResponse);
                return await Task.FromResult(new EmployerPayeSchemes());
            }

            var employerPayeSchemes = new EmployerPayeSchemes(accountsResponse.AccountId, resourceList.Select(x => x.Id.Trim().ToUpper()).ToList(), (HttpStatusCode)accountsResponse.HttpStatusCode);
            var allEmployerPayeSchemes = new StringBuilder();
            foreach (var payeScheme in employerPayeSchemes.PayeSchemes)
            {
                // Concatenate the list of PayeSchemes into a single string to store in the accounts response table
                allEmployerPayeSchemes.Append($", {payeScheme}");
            }

            // We've concatenated the strings with a leading comma so need to remove the leading comma at the start of the string
            var responsePayeSchemes = allEmployerPayeSchemes.ToString();
            responsePayeSchemes = responsePayeSchemes.Remove(0, 1);

            accountsResponse.PayeSchemes = responsePayeSchemes;
            await Save(accountsResponse);

            return employerPayeSchemes;
        }

        private async Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
        {
            if (!_hostingEnvironment.IsDevelopment() && !_httpClient.BaseAddress.IsLoopback)
            {
                var accessToken = await _azureClientCredentialHelper.GetAccessTokenAsync(_configuration.Identifier);
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        private async Task Save(AccountsResponse accountsResponse)
        {
            if (accountsResponse == null)
            {
                _logger.LogError($"LearnerService.Save(): ERROR: The accountsResponse model is null.");
                return;
            }

            await _repository.InsertOrUpdate(accountsResponse);
        }

        private async Task HandleException(Data.Models.EmploymentCheck employmentCheck, Exception e)
        {
            var accountsResponse = await InitialiseAccountResponseModel(employmentCheck);
            accountsResponse.HttpResponse = e.Message;
            await Save(accountsResponse);
        }
    }
}