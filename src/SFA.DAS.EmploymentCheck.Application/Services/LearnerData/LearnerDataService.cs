using Ardalis.GuardClauses;
using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmploymentCheck.Application.Common;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using SFA.DAS.EmploymentCheck.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Application.Interfaces.LearnerData;

namespace SFA.DAS.EmploymentCheck.Application.Services.LearnerData
{
    public class LearnerDataService
        : ILearnerDataService
    {
        #region Private members
        private const string ThisClassName = "\n\nLearnerService";
        private const string ErrorMessagePrefix = "[*** ERROR ***]";

        private readonly ILogger<LearnerDataService> _logger;
        private readonly ILearnerDataTokenService _learnerDataTokenService;
        private readonly IHttpClientFactory _httpFactory;
        private readonly DcApiSettings _dcApiSettings;

        private const string AzureResource = "https://database.windows.net/"; // TODO: move to config
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        #endregion Private members

        #region Constructors
        public LearnerDataService(
            ILogger<LearnerDataService> logger,
            ILearnerDataTokenService dcTokenService,
            IHttpClientFactory httpFactory,
            IOptions<DcApiSettings> dcApiSettings,
            AzureServiceTokenProvider azureServiceTokenProvider,
            ApplicationSettings applicationSettings
            )
        {
            _logger = logger;
            _connectionString = applicationSettings.DbConnectionString;
            _learnerDataTokenService = dcTokenService;
            _httpFactory = httpFactory;
            _dcApiSettings = dcApiSettings.Value;
            _azureServiceTokenProvider = azureServiceTokenProvider;
        }
        #endregion Constructors

        #region GetNiNumbers
        public async Task<IList<LearnerNiNumber>> GetNiNumbers(IList<Domain.Entities.EmploymentCheck> apprenticeshipEmploymentChecks)
        {
            var thisMethodName = $"{nameof(ThisClassName)}.GetNiNumbers()";

            Guard.Against.NullOrEmpty(apprenticeshipEmploymentChecks, nameof(apprenticeshipEmploymentChecks));

            IList<LearnerNiNumber> learnerNiNumbers = null;
            try
            {
                var token = GetDcToken().Result;

                // Call the Nino API
                learnerNiNumbers = await GetNiNumbers(apprenticeshipEmploymentChecks, token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(learnerNiNumbers);
        }

        private async Task<IList<LearnerNiNumber>> GetNiNumbers(IList<Domain.Entities.EmploymentCheck> learners, AuthResult token)
        {
            var thisMethodName = $"{nameof(LearnerDataService)}.GetNiNumbers()";

            _logger.LogInformation($"{thisMethodName}: Getting Ni Numbers for {learners.Count} apprentices");

            var timer = new Stopwatch();
            timer.Start();

            var checkedLearners = new List<LearnerNiNumber>();

            var taskList = new List<Task<LearnerNiNumber>>();

            foreach (var learner in learners)
            {
                taskList.Add(SendIndividualRequest(learner, token));

                var responses = await Task.WhenAll(taskList);
                foreach(var response in responses)
                {
                    if(response != null && response.NiNumber != null)
                    {
                        checkedLearners.AddRange(responses);
                    }
                }

                taskList.Clear();
            }

            timer.Stop();
            _logger.LogInformation($"{thisMethodName}: Got Ni Numbers for {learners.Count} apprentices. {timer}ms elapsed");

            return checkedLearners;
        }

        private async Task<LearnerNiNumber> SendIndividualRequest(Domain.Entities.EmploymentCheck employmentCheck, AuthResult token)
        {
            var thisMethodName = $"{nameof(LearnerDataService)}.SendIndividualRequest()";

            LearnerNiNumber learnerNiNumber = null;
            using (var client = _httpFactory.CreateClient("LearnerNiApi"))
            {
                client.BaseAddress = new Uri(_dcApiSettings.BaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                var url = "/api/v1/ilr-data/learnersNi/2122?ulns=" + learner.Uln;

                try
                {
                    _logger.LogInformation($"{thisMethodName} Executing Http Get Request to [{url}].");

                    var response = await client.GetAsync(url);
                    if (response == null)
                    {
                        _logger.LogInformation($"\n\n{thisMethodName}: response code received from Data Collections API is NULL");

                        await StoreDataCollectionsResponse(new DataCollectionsResponse(
                            employmentCheck.Id,
                            employmentCheck.CorrelationId,
                            employmentCheck.Uln,
                            "NULL", // NiNumber
                            "NULL", // HttpResponse
                            0));    // HttpStatusCode
                        throw new ArgumentNullException(nameof(response));
                    }

                    response.EnsureSuccessStatusCode(); // throws an exception if IsSuccessStatusCode property is false
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        _logger.LogInformation($"\n\n{thisMethodName}: response code received from Data Collections is [{response.StatusCode}]");

                        await StoreDataCollectionsResponse(new DataCollectionsResponse(
                            employmentCheck.Id,
                            employmentCheck.CorrelationId,
                            employmentCheck.Uln,
                            "NULL", // NiNumber
                            response.ToString(),
                            (short)response.StatusCode));
                        throw new ArgumentOutOfRangeException(nameof(response.StatusCode));
                    }

                    var content = await response.Content.ReadAsStreamAsync();
                    if (content == null || content.Length == 0)
                    {
                        _logger.LogInformation($"\n\n{thisMethodName}: response content received from Data Collections is NULL or zero length");

                        await StoreDataCollectionsResponse(new DataCollectionsResponse(
                            employmentCheck.Id,
                            employmentCheck.CorrelationId,
                            employmentCheck.Uln,
                            "NULL", // NiNumber
                            response.ToString(),
                            (short)response.StatusCode));
                        throw new ArgumentNullException(nameof(content));
                    }

                    var learnerNiNumbers = await JsonSerializer.DeserializeAsync<List<LearnerNiNumber>>(content);
                    if (learnerNiNumbers == null)
                    {
                        _logger.LogInformation($"\n\n{thisMethodName}: deserialised response content received from Data Collections is NULL or zero length");

                        await StoreDataCollectionsResponse(new DataCollectionsResponse(
                            employmentCheck.Id,
                            employmentCheck.CorrelationId,
                            employmentCheck.Uln,
                            "NULL", // NiNumber
                            response.ToString(),
                            (short)response.StatusCode));
                        throw new ArgumentNullException(nameof(learnerNiNumbers));
                    }

                    learnerNiNumber = learnerNiNumbers.FirstOrDefault();
                    _logger.LogInformation($"\n\n{thisMethodName}: deserialised response content NiNumber for Uln [{employmentCheck.Uln}] received from Data Collections.");

                    await StoreDataCollectionsResponse(new DataCollectionsResponse(
                        employmentCheck.Id,
                        employmentCheck.CorrelationId,
                        employmentCheck.Uln,
                        learnerNiNumber.NiNumber,
                        response.ToString(),
                        (short)response.StatusCode));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
                }
            }
            return learnerNiNumber;
        }

        private async Task<AuthResult> GetDcToken()
        {
            var thisMethodName = $"{nameof(LearnerDataService)}.GetDcToken()";

            AuthResult result = null;
            try
            {

                result = await _learnerDataTokenService.GetTokenAsync(
                    $"https://login.microsoftonline.com/{_dcApiSettings.Tenant}",
                    "client_credentials",
                    _dcApiSettings.ClientSecret,
                    _dcApiSettings.ClientId,
                    _dcApiSettings.IdentifierUri);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
                result = new AuthResult();
            }

            return result;
        }
        #endregion GetNiNumbers

        #region StoreDataCollectionsResponse
        public async Task<int> StoreDataCollectionsResponse(
            DataCollectionsResponse dataCollectionsResponse)
        {
            var thisMethodName = $"{nameof(LearnerDataService)}.StoreDataCollectionsResponse()";

            int result = 0;
            try
            {
                if (dataCollectionsResponse != null)
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
                                        parameter.Add("@employmentCheckId", dataCollectionsResponse.EmploymentCheckId, DbType.Int64);
                                        parameter.Add("@correlationId", dataCollectionsResponse.CorrelationId, DbType.Guid);
                                        parameter.Add("@uln", dataCollectionsResponse.Uln, DbType.Int64);
                                        parameter.Add("@niNumber", dataCollectionsResponse.NiNumber, DbType.String);
                                        parameter.Add("@httpResponse", dataCollectionsResponse.HttpResponse, DbType.String);
                                        parameter.Add("@httpStatusCode", dataCollectionsResponse.HttpStatusCode, DbType.Int16);
                                        parameter.Add("@createdOn", DateTime.Now, DbType.DateTime);

                                        await sqlConnection.ExecuteAsync(
                                            "INSERT [Cache].[DataCollectionsResponse] " +
                                            "       ( ApprenticeEmploymentCheckId,  CorrelationId,  Uln,  NiNumber,  HttpResponse,  HttpStatusCode,  CreatedOn) " +
                                            "VALUES (@apprenticeEmploymentCheckId, @correlationId, @uln, @niNumber, @httpResponse, @httpStatusCode, @createdOn)",
                                            parameter,
                                            commandType: CommandType.Text);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
                                    }
                                }
                            }
                            else
                            {
                                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Sql Connection is NULL");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(result);
        }
        #endregion StoreDataCollectionsResponse

        #region Stub data
        public async Task<IList<LearnerNiNumber>> GetLearnerNiNumbersStub(IList<Domain.Entities.EmploymentCheck> apprenticeEmploymentChecks)
        {
            var thisMethodName = $"{ThisClassName}.GetApprenticesNiNumber()";

            IList<LearnerNiNumber> apprenticesNiNumber = new List<LearnerNiNumber>();
            if (apprenticeEmploymentChecks != null &&
                apprenticeEmploymentChecks.Count > 0)
            {
                foreach (var apprenticeEmploymentCheck in apprenticeEmploymentChecks)
                {
                    var apprenticeNiNumber = await FindApprenticeNiNumberStub(apprenticeEmploymentCheck);
                    apprenticesNiNumber.Add(apprenticeNiNumber);
                }
            }

            _logger.LogInformation($"{thisMethodName}: returned {apprenticesNiNumber.Count} NI Numbers.");
            return await Task.FromResult(apprenticesNiNumber);
        }

        public Task<IList<LearnerNiNumber>> GetNiNumbersStub(IList<Domain.Entities.EmploymentCheck> employmentCheckBatch)
        {
            throw new System.NotImplementedException();
        }

        private async Task<LearnerNiNumber> FindApprenticeNiNumberStub(Domain.Entities.EmploymentCheck apprenticeEmploymentCheck)
        {
            var uln = apprenticeEmploymentCheck.Uln;
            var niNumber = "NI" + apprenticeEmploymentCheck.Uln.ToString();

            var apprenticeNiNumber = new LearnerNiNumber
            {
                Uln = uln,
                NiNumber = niNumber
            };

            return await Task.FromResult(apprenticeNiNumber);
        }

        private async Task<LearnerNiNumber> FindApprenticeNiNumber2Stub(Domain.Entities.EmploymentCheck apprenticeEmploymentCheck)
        {
            LearnerNiNumber apprenticeNiNumber;

            switch (apprenticeEmploymentCheck.ApprenticeshipId)
            {
                case 1:
                    apprenticeNiNumber = new LearnerNiNumber
                    {
                        Uln = 1000000001,
                        NiNumber = "NI1000000001"
                    };
                    break;

                case 2:
                    apprenticeNiNumber = new LearnerNiNumber
                    {
                        Uln = 2000000002,
                        NiNumber = "NI2000000002"
                    };
                    break;

                case 3:
                    apprenticeNiNumber = new LearnerNiNumber
                    {
                        Uln = 3000000003,
                        NiNumber = "NI3000000003"
                    };
                    break;

                case 4:
                    apprenticeNiNumber = new LearnerNiNumber
                    {
                        Uln = 4000000004,
                        NiNumber = "NI4000000004"
                    };
                    break;

                case 5:
                    apprenticeNiNumber = new LearnerNiNumber
                    {
                        Uln = 5000000005,
                        NiNumber = "NI5000000005"
                    };
                    break;

                case 6:
                    apprenticeNiNumber = new LearnerNiNumber
                    {
                        Uln = 6000000006,
                        NiNumber = "NI6000000006"
                    };
                    break;

                case 7:
                    apprenticeNiNumber = new LearnerNiNumber
                    {
                        Uln = 7000000007,
                        NiNumber = "NI7000000007"
                    };
                    break;

                case 8:
                    apprenticeNiNumber = new LearnerNiNumber
                    {
                        Uln = 8000000008,
                        NiNumber = "NI8000000008"
                    };
                    break;

                case 9:
                    apprenticeNiNumber = new LearnerNiNumber
                    {
                        Uln = 9000000009,
                        NiNumber = "NI9000000009"
                    };
                    break;

                default:
                    apprenticeNiNumber = new LearnerNiNumber
                    {
                        Uln = 1000000001,
                        NiNumber = "NI1000000001"
                    };
                    break;
            }

            return await Task.FromResult(apprenticeNiNumber);
        }
        #endregion Stub data
    }
}