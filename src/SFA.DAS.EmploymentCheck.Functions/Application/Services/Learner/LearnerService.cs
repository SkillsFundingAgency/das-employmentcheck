using Ardalis.GuardClauses;
using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Learner
{
    public class LearnerService
        : ILearnerService
    {
        #region Private members
        private const string LearnersNiUrl = "/api/v1/ilr-data/learnersNi/2122?ulns=";

        private readonly ILogger<LearnerService> _logger;
        private readonly IDcTokenService _dcTokenService;
        private readonly IHttpClientFactory _httpFactory;
        private readonly DcApiSettings _dcApiSettings;

        private const string AzureResource = "https://database.windows.net/"; // TODO: move to config
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

        #endregion Private members

        #region Constructors
        public LearnerService(
            ILogger<LearnerService> logger,
            IDcTokenService dcTokenService,
            IHttpClientFactory httpFactory,
            IOptions<DcApiSettings> dcApiSettings,
            AzureServiceTokenProvider azureServiceTokenProvider,
            ApplicationSettings applicationSettings
            )
        {
            _logger = logger;
            _connectionString = applicationSettings.DbConnectionString;
            _dcTokenService = dcTokenService;
            _httpFactory = httpFactory;
            _dcApiSettings = dcApiSettings.Value;
            _azureServiceTokenProvider = azureServiceTokenProvider;
        }
        #endregion Constructors

        #region GetNiNumbers

        public async Task<IList<LearnerNiNumber>> GetNiNumbers(IList<Models.EmploymentCheck> employmentCheckBatch)
        {
            Guard.Against.NullOrEmpty(employmentCheckBatch, nameof(employmentCheckBatch));

            var token = GetDcToken().Result;

            var learnerNiNumbers = await GetNiNumbers(employmentCheckBatch, token);

            return await Task.FromResult(learnerNiNumbers);
        }

        private async Task<IList<LearnerNiNumber>> GetNiNumbers(ICollection<Models.EmploymentCheck> learners, AuthResult token)
        {
            var checkedLearners = new List<LearnerNiNumber>();
            var taskList = new List<Task<LearnerNiNumber>>();

            foreach (var learner in learners)
            {
                taskList.Add(SendIndividualRequest(learner, token));

                var responses = await Task.WhenAll(taskList);
                foreach(var response in responses)
                {
                    if (response is {NiNumber: { }})
                    {
                        checkedLearners.AddRange(responses);
                    }
                }

                taskList.Clear();
            }

            return checkedLearners;
        }

        private async Task<LearnerNiNumber> SendIndividualRequest(Models.EmploymentCheck learner, AuthResult token)
        {
            var thisMethodName = $"{nameof(LearnerService)}.SendIndividualRequest";

            using var client = _httpFactory.CreateClient("LearnerNiApi");
            client.BaseAddress = new Uri(_dcApiSettings.BaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var url = LearnersNiUrl + learner.Uln;

            var response = await client.GetAsync(url);
            if (response == null)
            {
                await StoreDataCollectionsResponse(new DataCollectionsResponse(
                    learner.Id,
                    learner.CorrelationId,
                    learner.Uln,
                    "NULL", // NiNumber
                    "NULL", // HttpResponse
                    0)); // HttpStatusCode
                throw new ArgumentNullException(
                    $"\n\n{thisMethodName}: response received from Data Collections API is NULL");
            }

            if (!response.IsSuccessStatusCode)
            {
                await StoreDataCollectionsResponse(new DataCollectionsResponse(
                    learner.Id,
                    learner.CorrelationId,
                    learner.Uln,
                    "NULL", // NiNumber
                    response.ToString(),
                    (short) response.StatusCode));
                throw new ArgumentException(
                    $"\n\n{thisMethodName}: Data Collections API call failed");
            }

            var content = await response.Content.ReadAsStreamAsync();
            if (content == null || content.Length == 0)
            {
                await StoreDataCollectionsResponse(new DataCollectionsResponse(
                    learner.Id,
                    learner.CorrelationId,
                    learner.Uln,
                    "NULL", // NiNumber
                    response.ToString(),
                    (short) response.StatusCode));
            }

            var learnerNiNumbers = await JsonSerializer.DeserializeAsync<List<LearnerNiNumber>>(content);
            if (learnerNiNumbers == null)
            {
                await StoreDataCollectionsResponse(new DataCollectionsResponse(
                    learner.Id,
                    learner.CorrelationId,
                    learner.Uln,
                    "NULL", // NiNumber
                    response.ToString(),
                    (short) response.StatusCode));
                throw new ArgumentNullException($"\n\n{thisMethodName}: deserialised response content received from Data Collections is NULL or zero length");
            }

            var learnerNiNumber = learnerNiNumbers.FirstOrDefault();

            await StoreDataCollectionsResponse(new DataCollectionsResponse(
                learner.Id,
                learner.CorrelationId,
                learner.Uln,
                learnerNiNumber?.NiNumber,
                response.ToString(),
                (short) response.StatusCode));

            return learnerNiNumber;
        }

        private async Task<AuthResult> GetDcToken()
        {
            var result = await _dcTokenService.GetTokenAsync(
                $"https://login.microsoftonline.com/{_dcApiSettings.Tenant}",
                "client_credentials",
                _dcApiSettings.ClientSecret,
                _dcApiSettings.ClientId,
                _dcApiSettings.IdentifierUri);

            return result;
        }

        #endregion GetNiNumbers

        #region StoreDataCollectionsResponse

        public async Task<int> StoreDataCollectionsResponse(DataCollectionsResponse dataCollectionsResponse)
        {
            if (dataCollectionsResponse == null) return await Task.FromResult(0);

            var dbConnection = new DbConnection();
            await using var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                AzureResource,
                _azureServiceTokenProvider);
            Guard.Against.Null(sqlConnection, nameof(sqlConnection));

            await sqlConnection.OpenAsync();

            var parameter = new DynamicParameters();
            parameter.Add("@apprenticeEmploymentCheckId", dataCollectionsResponse.ApprenticeEmploymentCheckId, DbType.Int64);
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

            return await Task.FromResult(0);
        }

        #endregion StoreDataCollectionsResponse
    }
}