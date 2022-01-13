using Ardalis.GuardClauses;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.HashingService;
using System;
using System.Collections.Generic;
using System.Data;
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
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        #endregion Private memebers

        #region Constructors
        public EmployerAccountService(
            ILogger<EmployerAccountService> logger,
            EmployerAccountApiConfiguration apiConfiguration,
            ApplicationSettings applicationSettings,
            IHashingService hashingService,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment hostingEnvironment,
            IAzureClientCredentialHelper azureClientCredentialHelper, 
            AzureServiceTokenProvider azureServiceTokenProvider)
        {
            _logger = logger;
            _connectionString = applicationSettings.DbConnectionString;
            _configuration = apiConfiguration;
            _hashingService = hashingService;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(apiConfiguration.Url);
            _hostingEnvironment = hostingEnvironment;
            _azureClientCredentialHelper = azureClientCredentialHelper;
            _azureServiceTokenProvider = azureServiceTokenProvider;
        }
        #endregion Constructors

        #region GetPayeSchemes

        public async Task<ResourceList> GetPayeSchemes(Models.EmploymentCheck apprenticeEmploymentCheck)
        {
            var resourceList = await Get<ResourceList>(apprenticeEmploymentCheck);

            return resourceList;
        }

        #endregion GetPayeSchemes

        #region Get

        public async Task<TResponse> Get<TResponse>(Models.EmploymentCheck apprenticeEmploymentCheck)
        {
            const string thisMethodName = "EmployerAccountApiClient.Get";

            var hashedAccountId = _hashingService.HashValue(apprenticeEmploymentCheck.AccountId);
            var url = $"api/accounts/{hashedAccountId}/payeschemes";

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            await AddAuthenticationHeader(httpRequestMessage);

            var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            if (response == null)
            {
                // The API call didn't return a response
                // Log it and throw and exception to skip the rest of the processing
                _logger.LogError(
                    $"\n\n{thisMethodName}: response received from Employer Accounts API is NULL");

                await StoreAccountsResponse(new AccountsResponse(
                    apprenticeEmploymentCheck.Id,
                    apprenticeEmploymentCheck.CorrelationId,
                    apprenticeEmploymentCheck.AccountId,
                    "NULL", // payeSchemes
                    "NULL", // HttpResponse
                    0)); // HttpStatusCode
                throw new ArgumentNullException(nameof(response));
            }

            // throws an exception if the IsSuccessStatusCode property is false
            // so we check the IsSuccessStatsCode directly to avoid the exception
            // enabling us to store the response
            if (response.IsSuccessStatusCode == false)
            {
                // The API call returned an none successful code
                // Log it and throw and exception to skip the rest of the processing
                _logger.LogError(
                    $"\n\n{thisMethodName}: response IsSuccessStatusCode returned from the Employer Accounts API is false the status code is [{response.StatusCode}]");

                await StoreAccountsResponse(new AccountsResponse(
                    apprenticeEmploymentCheck.Id,
                    apprenticeEmploymentCheck.CorrelationId,
                    apprenticeEmploymentCheck.AccountId,
                    "NULL", // payeSchemes
                    response.ToString(),
                    (short) response.StatusCode));
                throw
                    new InvalidOperationException(
                        nameof(response)); // TODO: Create a custom business exception for this condition
            }

           
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(json))
            {
                // read of content failed
                _logger.LogError(
                    $"\n\n{thisMethodName}: converting the response Content to string returned an Empty/Null string (the status code is [{response.StatusCode}])");

                await StoreAccountsResponse(new AccountsResponse(
                    apprenticeEmploymentCheck.Id,
                    apprenticeEmploymentCheck.CorrelationId,
                    apprenticeEmploymentCheck.AccountId,
                    "NULL", // payeSchemes
                    response.ToString(),
                    (short) response.StatusCode));
                throw
                    new InvalidOperationException(
                        nameof(response)); // TODO: Create a custom business exception for this condition
            }

            // Deserialise the content
            var content = JsonConvert.DeserializeObject<TResponse>(json);
            if (content == null)
            {
                // Deserialisation of content failed
                _logger.LogError($"\n\n{thisMethodName}: deseriaising content failed.)");

                await StoreAccountsResponse(new AccountsResponse(
                    apprenticeEmploymentCheck.Id,
                    apprenticeEmploymentCheck.CorrelationId,
                    apprenticeEmploymentCheck.AccountId,
                    "NULL", // payeSchemes
                    response.ToString(),
                    (short) response.StatusCode));
                throw
                    new InvalidOperationException(
                        nameof(response)); // TODO: Create a custom business exception for this condition
            }

            // get the resource list
            var resourceList = new ResourceList((IEnumerable<ResourceViewModel>) await Task.FromResult(content));

            var payeSchemes = new EmployerPayeSchemes(apprenticeEmploymentCheck.AccountId,
                resourceList.Select(x => x.Id).ToList());

            var allPayeSchemes = new StringBuilder();
            foreach (var payeScheme in payeSchemes.PayeSchemes)
            {
                allPayeSchemes.Append($", {payeScheme}");
            }

            // remove comma at start of string
            var responsePayeSchemes = allPayeSchemes.ToString();
            responsePayeSchemes = responsePayeSchemes.Remove(0, 1);

            await StoreAccountsResponse(new AccountsResponse(
                apprenticeEmploymentCheck.Id,
                apprenticeEmploymentCheck.CorrelationId,
                apprenticeEmploymentCheck.AccountId,
                responsePayeSchemes,
                response.ToString(),
                (short) response.StatusCode));

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

        #endregion Get

        #region StoreAccountsResponse

        public async Task<int> StoreAccountsResponse(
            AccountsResponse accountsResponse)
        {
            Guard.Against.Null(accountsResponse, nameof(accountsResponse));

            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            await sqlConnection.OpenAsync();

            var parameter = new DynamicParameters();
            parameter.Add("@apprenticeEmploymentCheckId", accountsResponse.ApprenticeEmploymentCheckId, DbType.Int64);
            parameter.Add("@correlationId", accountsResponse.CorrelationId, DbType.Guid);
            parameter.Add("@accountId", accountsResponse.AccountId, DbType.Int64);
            parameter.Add("@payeSchemes", accountsResponse.PayeSchemes, DbType.String);
            parameter.Add("@httpResponse", accountsResponse.HttpResponse, DbType.String);
            parameter.Add("@httpStatusCode", accountsResponse.HttpStatusCode, DbType.Int16);
            parameter.Add("@createdOn", DateTime.Now, DbType.DateTime);

            await sqlConnection.ExecuteScalarAsync(
                "INSERT [Cache].[AccountsResponse] " +
                "       ( ApprenticeEmploymentCheckId,  CorrelationId,  AccountId,  PayeSchemes,  HttpResponse,  HttpStatusCode,  CreatedOn) " +
                "VALUES (@apprenticeEmploymentCheckId, @correlationId, @accountId, @payeSchemes, @httpResponse, @httpStatusCode, @createdOn)",
                parameter,
                commandType: CommandType.Text);

            return await Task.FromResult(0);
        }

        #endregion StoreAccountsResponse

    }
}