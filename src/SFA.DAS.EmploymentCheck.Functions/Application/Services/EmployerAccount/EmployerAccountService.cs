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
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount
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

        public async Task<EmployerPayeSchemes> GetEmployerPayeSchemes(Models.EmploymentCheck employmentCheck)
        {
            // Setup config for Accounts Api call
            HttpRequestMessage httpRequestMessage = await SetupAccountsApiConfig(employmentCheck);

            // Call the Accounts Api to get the PayeSchemes
            var payeSchemes = await ExecuteAccountsApiCall(employmentCheck, httpRequestMessage).ConfigureAwait(false);

            // Return the paye schemes to the caller
            return payeSchemes ?? new EmployerPayeSchemes();
        }

        private async Task<HttpRequestMessage> SetupAccountsApiConfig(Models.EmploymentCheck employmentCheck)
        {
            var hashedAccountId = _hashingService.HashValue(employmentCheck.AccountId);
            var url = $"api/accounts/{hashedAccountId}/payeschemes";
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            await AddAuthenticationHeader(httpRequestMessage);
            return httpRequestMessage;
        }

        private async Task<EmployerPayeSchemes> ExecuteAccountsApiCall(Models.EmploymentCheck employmentCheck, HttpRequestMessage httpRequestMessage)
        {
            EmployerPayeSchemes employerPayeSchemes = null;
            try
            {
                // Call the Accounts Api
                var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

                // Get the EmployerPayeSchemes from the response
                employerPayeSchemes = await GetApiResponsePayeSchemes(employmentCheck, response);
            }
            catch (Exception e)
            {
                await HandleException(employmentCheck, e);
            }

            // Return the EmployerPayeSchemes
            return employerPayeSchemes;
        }

        private async Task<EmployerPayeSchemes> GetApiResponsePayeSchemes(
            Models.EmploymentCheck employmentCheck,
            HttpResponseMessage httpResponseMessage
        )
        {
            // This has already been null checked earlier in the call-chain so should not be null
            Guard.Against.Null(employmentCheck, nameof(employmentCheck));

            // Create an 'AccountsResponse' model to hold the api response data to store in the database
            var accountsResponse = await InitialiseAccountResponseModel(employmentCheck);

            // Check we have response data
            if (httpResponseMessage == null)
            {
                await Save(accountsResponse);
                return await Task.FromResult(new EmployerPayeSchemes());
            }

            // Store the api response data in the accounts response model
            accountsResponse.HttpResponse = httpResponseMessage.ToString();
            accountsResponse.HttpStatusCode = (short)httpResponseMessage.StatusCode;

            // Check the response success status code to determine if the api call succeeded
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                // The call wasn't successful, save the api response data and return an empty EmployerPayeSchemes
                await Save(accountsResponse);
                return await Task.FromResult(new EmployerPayeSchemes());
            }

            // The api call was succesful, read the response content
            var jsonContent = await ReadResponseContent(httpResponseMessage, accountsResponse);

            // Deserialise the employer paye schemes from the content
            var employerPayeSchemes = await DeserialiseContent(jsonContent, accountsResponse);

            // return the employer schemes to the caller
            return employerPayeSchemes;
        }

        private async Task<AccountsResponse> InitialiseAccountResponseModel(Models.EmploymentCheck employmentCheck)
        {
            return await Task.FromResult(new AccountsResponse(
                employmentCheck.Id,
                employmentCheck.CorrelationId,
                employmentCheck.AccountId,
                string.Empty,                                   // PayeSchemes,
                string.Empty,                                   // Response
                (short)HttpStatusCode.InternalServerError));    // Http Status Code
        }

        private async Task<string> ReadResponseContent(
            HttpResponseMessage httpResponseMessage,
            AccountsResponse accountsResponse
        )
        {
            // This has already been null checked earlier in the call-chain so should not be null
            Guard.Against.Null(accountsResponse, nameof(accountsResponse));

            // This has already been null checked earlier in the call-chain so should not be null
            Guard.Against.Null(httpResponseMessage, nameof(httpResponseMessage));

            // Read the content
            var json = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(json))
            {
                // Nothing to read, store the accounts response and return an empty 'content' string
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
            // Check the jsonContent has a value
            if (string.IsNullOrEmpty(jsonContent))
            {
                await Save(accountsResponse);
                return await Task.FromResult(new EmployerPayeSchemes());
            }

            // This has already been null checked earlier in the call-chain so should not be null
            Guard.Against.Null(accountsResponse, nameof(accountsResponse));

            // Deserialise the json content
            var resourceList = JsonConvert.DeserializeObject<ResourceList>(jsonContent);
            if (resourceList == null || !resourceList.Any())
            {
                // Nothing to deserialise, store the accounts response and return an empty EmployerPayeSchemes
                await Save(accountsResponse);
                return await Task.FromResult(new EmployerPayeSchemes());
            }

            // Get the EmployerPayeSchemes from the ResourceList
            var employerPayeSchemes = new EmployerPayeSchemes(accountsResponse.AccountId, resourceList.Select(x => x.Id.Trim().ToUpper()).ToList());

            // Concatenate the list of PayeSchemes into a string for storing in the accounts response table
            var allEmployerPayeSchemes = new StringBuilder();
            foreach (var payeScheme in employerPayeSchemes.PayeSchemes)
            {
                allEmployerPayeSchemes.Append($", {payeScheme}");
            }

            // Remove the leading comma
            var responsePayeSchemes = allEmployerPayeSchemes.ToString();
            responsePayeSchemes = responsePayeSchemes.Remove(0, 1);

            // store the paye schemes string in the accountsResponse and save the accountsResponse to the database
            accountsResponse.PayeSchemes = responsePayeSchemes;
            await Save(accountsResponse);

            // Return the employer paye schemes to the caller
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

            // Temporary work-around try/catch for handling duplicate inserts until we switch to single message processing
            try
            {
                await _repository.Save(accountsResponse);
            }
            catch
            {
                // No logging, we're not interested in storing errors about duplicates at the moment
            }
        }

        private async Task HandleException(Models.EmploymentCheck employmentCheck, Exception e)
        {
            // Create an 'AccountsResponse' model to hold the api response data to store in the database
            var accountsResponse = await InitialiseAccountResponseModel(employmentCheck);

            // Store the exception message in the AccountsResponse model and store in the database
            accountsResponse.HttpResponse = e.Message;
            await Save(accountsResponse);
        }
    }
}