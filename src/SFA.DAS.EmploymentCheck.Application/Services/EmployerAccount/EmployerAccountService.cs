using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Application.Common;
using SFA.DAS.EmploymentCheck.Application.Interfaces.EmployerAccount;
using SFA.DAS.EmploymentCheck.Application.Common.Models;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using SFA.DAS.HashingService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount
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

        private const string AzureResource = "https://database.windows.net/"; // TODO: move to config
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
        public async Task<ResourceList> GetPayeSchemes(Domain.Entities.EmploymentCheck apprenticeEmploymentCheck)
        {
            var thisMethodName = $"{nameof(EmployerAccountService)}.GetPayeSchemes()";

            ResourceList resourceList = null;
            try
            {
                resourceList = await Get<ResourceList>(apprenticeEmploymentCheck);
                if (resourceList != null && resourceList.Count > 0)
                {
                    _logger.LogInformation($"{thisMethodName}: returned {resourceList.Count} PAYE schemes");

                    var payeSchemes = new EmployerPayeSchemes(apprenticeEmploymentCheck.AccountId, resourceList.Select(x => x.Id).ToList());
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: returned null/zero PAYE schemes");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}\n\n Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return resourceList;
        }
        #endregion GetPayeSchemes

        #region Get
        public async Task<TResponse> Get<TResponse>(Domain.Entities.EmploymentCheck apprenticeEmploymentCheck)
        {
            var thisMethodName = "EmployerAccountApiClient.Get()";
            string json = string.Empty;
            TResponse content = default(TResponse);

            var hashedAccountId = _hashingService.HashValue(apprenticeEmploymentCheck.AccountId);
            var url = $"api/accounts/{hashedAccountId}/payeschemes";

            var accountsResponseId = 0;
            try
            {
                _logger.LogInformation($"{thisMethodName} Executing Http Get Request to [{url}].");
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                await AddAuthenticationHeader(httpRequestMessage);

                var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                if (response == null)
                {
                    // The API call didn't return a response
                    // Log it and throw and exception to skip the rest of the processing
                    _logger.LogInformation($"\n\n{thisMethodName}: response code received from Employer Accounts API is NULL");

                    await StoreAccountsResponse(new AccountsResponse(
                        apprenticeEmploymentCheck.Id,
                        apprenticeEmploymentCheck.CorrelationId,
                        apprenticeEmploymentCheck.AccountId,
                        "NULL",  // payeSchemes
                        "NULL", // HttpResponse
                        0));    // HttpStatusCode
                    throw new ArgumentNullException(nameof(response));
                }

                // response.EnsureSuccessStatusCode();
                // throws an exception if the IsSuccessStatusCode property is false
                // so we check the IsSuccessStatsCode directly to avoid the exception
                // enabling us to store the reposponse
                if(response.IsSuccessStatusCode == false)
                {
                    // The API call returned an none succesful code
                    // Log it and throw and exception to skip the rest of the processing
                    _logger.LogInformation($"\n\n{thisMethodName}: response IsSuccessStatusCode returned from the Employer Accounts API is false the status code is [{response.StatusCode}]");

                    await StoreAccountsResponse(new AccountsResponse(
                        apprenticeEmploymentCheck.Id,
                        apprenticeEmploymentCheck.CorrelationId,
                        apprenticeEmploymentCheck.AccountId,
                        "NULL",  // payeSchemes
                        response.ToString(),
                        (short)response.StatusCode));
                    throw new  InvalidOperationException(nameof(response)); // TODO: Create a custom business exception for this condition
                }

                // we have a succesful API call, process the API response
                _logger.LogInformation($"\n\n{thisMethodName}: Data Collections response IsSuccessStatusCode is true, status code is [{response.StatusCode}]");

                json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if(string.IsNullOrEmpty(json))
                {
                    // read of content failed
                    _logger.LogInformation($"\n\n{thisMethodName}: converting the response Content to string returned an Empty/Null string (the status code is [{response.StatusCode}])");

                    await StoreAccountsResponse(new AccountsResponse(
                        apprenticeEmploymentCheck.Id,
                        apprenticeEmploymentCheck.CorrelationId,
                        apprenticeEmploymentCheck.AccountId,
                        "NULL",  // payeSchemes
                        response.ToString(),
                        (short)response.StatusCode));
                    throw new InvalidOperationException(nameof(response)); // TODO: Create a custom business exception for this condition
                }

                // Deserialise the content
                content = JsonConvert.DeserializeObject<TResponse>(json);
                if(content == null)
                {
                    // Deserialisation of content failed
                    _logger.LogInformation($"\n\n{thisMethodName}: deseriaising content failed.)");

                    await StoreAccountsResponse(new AccountsResponse(
                        apprenticeEmploymentCheck.Id,
                        apprenticeEmploymentCheck.CorrelationId,
                        apprenticeEmploymentCheck.AccountId,
                        "NULL",  // payeSchemes
                        response.ToString(),
                        (short)response.StatusCode));
                    throw new InvalidOperationException(nameof(response)); // TODO: Create a custom business exception for this condition
                }

                // get the resource list
                ResourceList resourceList = new ResourceList((IEnumerable<ResourceViewModel>)await Task.FromResult(content));
                if(resourceList == null)
                {
                    // Conversion of deserialised content to resource list failed
                    _logger.LogInformation($"\n\n{thisMethodName}: conversion of deserialised content to resource list failed.)");

                    await StoreAccountsResponse(new AccountsResponse(
                        apprenticeEmploymentCheck.Id,
                        apprenticeEmploymentCheck.CorrelationId,
                        apprenticeEmploymentCheck.AccountId,
                        "NULL",  // payeSchemes
                        response.ToString(),
                        (short)response.StatusCode));
                    throw new InvalidOperationException(nameof(response)); // TODO: Create a custom business exception for this condition
                }

                //// get the Paye schemes
                var payeSchemes = new EmployerPayeSchemes(apprenticeEmploymentCheck.AccountId, resourceList.Select(x => x.Id).ToList());
                if (payeSchemes == null)
                {
                    // Conversion of deserialised content to resource list failed
                    _logger.LogInformation($"\n\n{thisMethodName}: conversion of deserialised content to paye schemes failed.)");

                    await StoreAccountsResponse(new AccountsResponse(
                        apprenticeEmploymentCheck.Id,
                        apprenticeEmploymentCheck.CorrelationId,
                        apprenticeEmploymentCheck.AccountId,
                        "NULL",  // payeSchemes
                        response.ToString(),
                        (short)response.StatusCode));
                    throw new InvalidOperationException(nameof(response)); // TODO: Create a custom business exception for this condition
                }

                StringBuilder allPayeSchemes = new StringBuilder();
                foreach (var payeScheme in payeSchemes.PayeSchemes)
                {
                    allPayeSchemes.Append($", {payeScheme}");
                }

                // remove comma at start of string
                string responsePayeSchemes = allPayeSchemes.ToString();
                responsePayeSchemes = responsePayeSchemes.Remove(0, 1);

                await StoreAccountsResponse(new AccountsResponse(
                     apprenticeEmploymentCheck.Id,
                     apprenticeEmploymentCheck.CorrelationId,
                     apprenticeEmploymentCheck.AccountId,
                     responsePayeSchemes,
                     response.ToString(),
                     (short)response.StatusCode));

                _logger.LogInformation($"{thisMethodName} Http Get Request returned {json}.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
                return JsonConvert.DeserializeObject<TResponse>("");
            }

            return content;
        }

        private async Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
        {
            var thisMethodName = "EmployerAccountApiClient.AddAuthenticationHeader()";

            if (!_hostingEnvironment.IsDevelopment() && !_httpClient.BaseAddress.IsLoopback)
            {
                try
                {
                    var accessToken = await _azureClientCredentialHelper.GetAccessTokenAsync(_configuration.Identifier);
                    httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
                }
            }
        }
    #endregion Get

        #region StoreAccountsResponse
    public async Task<int> StoreAccountsResponse(
            AccountsResponse accountsResponse)
        {
            var thisMethodName = $"{nameof(EmployerAccountService)}.StoreAccountsResponse()";

            int result = 0;
            try
            {
                if (accountsResponse != null)
                {
                    var dbConnection = new DbConnection();
                    if (dbConnection != null)
                    {
                        await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                            _logger,
                            _connectionString,
                            AzureResource,
                            _azureServiceTokenProvider))
                        {
                            if (sqlConnection != null)
                            {
                                await sqlConnection.OpenAsync();
                                {
                                    try
                                    {
                                        var parameter = new DynamicParameters();
                                        parameter.Add("@EmploymentCheckId", accountsResponse.EmploymentCheckId, DbType.Int64);
                                        parameter.Add("@correlationId", accountsResponse.CorrelationId, DbType.Guid);
                                        parameter.Add("@accountId", accountsResponse.AccountId, DbType.Int64);
                                        parameter.Add("@payeSchemes", accountsResponse.PayeSchemes, DbType.String);
                                        parameter.Add("@httpResponse", accountsResponse.HttpResponse, DbType.String);
                                        parameter.Add("@httpStatusCode", accountsResponse.HttpStatusCode, DbType.Int16);
                                        parameter.Add("@createdOn", DateTime.Now, DbType.DateTime);

                                        var id = await sqlConnection.ExecuteScalarAsync(
                                            "INSERT [SFA.DAS.EmploymentCheck.Database].[Cache].[AccountsResponse] " +
                                            "       ( EmploymentCheckId,  CorrelationId,  AccountId,  PayeSchemes,  HttpResponse,  HttpStatusCode,  CreatedOn) " +
                                            "VALUES (@EmploymentCheckId, @correlationId, @accountId, @payeSchemes, @httpResponse, @httpStatusCode, @createdOn)",
                                            parameter,
                                            commandType: CommandType.Text);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError($"{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(result);
        }
        #endregion StoreAccountsResponse

        #region Stub data
        public async Task<ResourceList> GetPayeSchemesStub(long accountId)
        {
            return await Task.FromResult(new ResourceList(new List<ResourceViewModel>
            {
                new ResourceViewModel
                {
                    Id = (accountId + 1000).ToString(), Href = $"PayeScheme{accountId}"
                }
            }));
        }

        public async Task<AccountDetailViewModel> GetEmployerAccountStub(long accountId)
        {
            var thisMethodName = MethodBase.GetCurrentMethod().Name;

            AccountDetailViewModel accountDetailViewModel;
            switch (accountId)
            {
                case 1:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 1,
                        StartingTransferAllowance = 1m,
                        RemainingTransferAllowance = 1m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "1001", Href = "PayeScheme1001" }
                        })
                    };
                    break;

                case 2:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 2,
                        StartingTransferAllowance = 2m,
                        RemainingTransferAllowance = 2m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "2001", Href = "PayeScheme2001" },
                            new ResourceViewModel { Id = "2002", Href = "PayeScheme2002" }
                        })
                    };
                    break;

                case 3:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 3,
                        StartingTransferAllowance = 3m,
                        RemainingTransferAllowance = 3m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "3001", Href = "PayeScheme3001" },
                            new ResourceViewModel { Id = "3002", Href = "PayeScheme3002" },
                            new ResourceViewModel { Id = "3003", Href = "PayeScheme3003" }
                        })
                    };
                    break;

                case 4:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 4,
                        StartingTransferAllowance = 4m,
                        RemainingTransferAllowance = 4m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "4001", Href = "PayeScheme4001" },
                            new ResourceViewModel { Id = "4002", Href = "PayeScheme4002" },
                            new ResourceViewModel { Id = "4003", Href = "PayeScheme4003" },
                            new ResourceViewModel { Id = "4004", Href = "PayeScheme4004" }
                        })
                    };
                    break;

                case 5:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 5,
                        StartingTransferAllowance = 5m,
                        RemainingTransferAllowance = 5m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "5001", Href = "PayeScheme5001" },
                            new ResourceViewModel { Id = "5002", Href = "PayeScheme5002" },
                            new ResourceViewModel { Id = "5003", Href = "PayeScheme5003" },
                            new ResourceViewModel { Id = "5004", Href = "PayeScheme5004" },
                            new ResourceViewModel { Id = "5005", Href = "PayeScheme5005" }
                        })
                    };
                    break;

                case 6:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 6,
                        StartingTransferAllowance = 6m,
                        RemainingTransferAllowance = 6m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "6001", Href = "PayeScheme6001" },
                            new ResourceViewModel { Id = "6002", Href = "PayeScheme6002" },
                            new ResourceViewModel { Id = "6003", Href = "PayeScheme6003" },
                            new ResourceViewModel { Id = "6004", Href = "PayeScheme6004" },
                            new ResourceViewModel { Id = "6005", Href = "PayeScheme6005" },
                            new ResourceViewModel { Id = "6006", Href = "PayeScheme6006" }
                        })
                    };
                    break;

                case 7:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 7,
                        StartingTransferAllowance = 7m,
                        RemainingTransferAllowance = 7m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "7001", Href = "PayeScheme7001" },
                            new ResourceViewModel { Id = "7002", Href = "PayeScheme7002" },
                            new ResourceViewModel { Id = "7003", Href = "PayeScheme7003" },
                            new ResourceViewModel { Id = "7004", Href = "PayeScheme7004" },
                            new ResourceViewModel { Id = "7005", Href = "PayeScheme7005" },
                            new ResourceViewModel { Id = "7006", Href = "PayeScheme7006" },
                            new ResourceViewModel { Id = "7007", Href = "PayeScheme7007" }
                        })
                    };
                    break;

                case 8:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 8,
                        StartingTransferAllowance = 8m,
                        RemainingTransferAllowance = 8m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "8001", Href = "PayeScheme8001" },
                            new ResourceViewModel { Id = "8002", Href = "PayeScheme8002" },
                            new ResourceViewModel { Id = "8003", Href = "PayeScheme8003" },
                            new ResourceViewModel { Id = "8004", Href = "PayeScheme8004" },
                            new ResourceViewModel { Id = "8005", Href = "PayeScheme8005" },
                            new ResourceViewModel { Id = "8006", Href = "PayeScheme8006" },
                            new ResourceViewModel { Id = "8007", Href = "PayeScheme8007" },
                            new ResourceViewModel { Id = "8008", Href = "PayeScheme8008" }
                        })
                    };
                    break;

                case 9:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 9,
                        StartingTransferAllowance = 9m,
                        RemainingTransferAllowance = 9m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "9001", Href = "PayeScheme9001" },
                            new ResourceViewModel { Id = "9002", Href = "PayeScheme9002" },
                            new ResourceViewModel { Id = "9003", Href = "PayeScheme9003" },
                            new ResourceViewModel { Id = "9004", Href = "PayeScheme9004" },
                            new ResourceViewModel { Id = "9005", Href = "PayeScheme9005" },
                            new ResourceViewModel { Id = "9006", Href = "PayeScheme9006" },
                            new ResourceViewModel { Id = "9007", Href = "PayeScheme9007" },
                            new ResourceViewModel { Id = "9008", Href = "PayeScheme9008" },
                            new ResourceViewModel { Id = "9009", Href = "PayeScheme9009" }
                        })
                    };
                    break;

                default:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 1,
                        StartingTransferAllowance = 1m,
                        RemainingTransferAllowance = 1m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "1001", Href = "PayeScheme1001" }
                        })
                    };
                    break;
            }

            _logger.LogInformation($"{thisMethodName}: returned {accountDetailViewModel.PayeSchemes.Count} PAYE schemes.");
            return await Task.FromResult(accountDetailViewModel);
        }

        #endregion Stub data
    }
}