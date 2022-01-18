using Ardalis.GuardClauses;
using Dapper;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private readonly IDataCollectionsResponseRepository _repository;
        #endregion Private members

        #region Constructors
        public LearnerService(
            ILogger<LearnerService> logger,
            IDcTokenService dcTokenService,
            IHttpClientFactory httpClientFactory,
            IOptions<DcApiSettings> dcApiSettings,
            AzureServiceTokenProvider azureServiceTokenProvider,
            ApplicationSettings applicationSettings,
            IDataCollectionsResponseRepository repository
        )
        {
            _logger = logger;
            _connectionString = applicationSettings.DbConnectionString;
            _dcTokenService = dcTokenService;
            _httpFactory = httpClientFactory;
            _dcApiSettings = dcApiSettings.Value;
            _azureServiceTokenProvider = azureServiceTokenProvider;
            _repository = repository;
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
            using var client = _httpFactory.CreateClient("LearnerNiApi");
            client.BaseAddress = new Uri(_dcApiSettings.BaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            var url = LearnersNiUrl + learner.Uln;

            // setup a default template 'response' to store the api response
            var dataCollectionsResponse = new DataCollectionsResponse(
                learner.Id,
                learner.CorrelationId,
                learner.Uln,
                string.Empty,   // NiNumber
                string.Empty,   // Response
                -1);            // Http Status Code

            // Call the DC api
            HttpResponseMessage response = null;
            IList<LearnerNiNumber> learnerNiNumbers = null;
            LearnerNiNumber learnerNiNumber = null;
            try
            {
                response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStreamAsync();
                learnerNiNumbers = await JsonSerializer.DeserializeAsync<List<LearnerNiNumber>>(content);
                learnerNiNumber = learnerNiNumbers.FirstOrDefault();

                dataCollectionsResponse.NiNumber = learnerNiNumber != null ? learnerNiNumber.NiNumber : string.Empty;
                dataCollectionsResponse.HttpResponse = response != null ? response.ToString() : "ERROR: LearnerService.SendIndividualRequest() - The call to the Data Collections API returned no response data.";
                dataCollectionsResponse.HttpStatusCode = (short)response.StatusCode;
            }
            catch (Exception e)
            {
                dataCollectionsResponse.HttpResponse = e.Message;
                _logger.LogError($"LearnerService.SendIndividualRequest(): ERROR: {dataCollectionsResponse.HttpResponse}");
            }
            finally
            {
                try
                {
                    await _repository.Save(dataCollectionsResponse);
                }
                catch (Exception e)
                {
                    _logger.LogError($"LearnerService.SendIndividualRequest(): ERROR: the DataCollections repository Save() method threw an Exception [{e}]");
                }
            }

            return learnerNiNumber != null ? learnerNiNumber : new LearnerNiNumber();
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
    }
}