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
using System.IO;
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
        private const string LearnersNiUrl = "/api/v1/ilr-data/learnersNi/2122?ulns=";
        private readonly ILogger<ILearnerService> _logger;
        private readonly IDcTokenService _dcTokenService;
        private readonly IHttpClientFactory _httpFactory;
        private readonly DcApiSettings _dcApiSettings;
        private readonly string _connectionString;
        private readonly AzureServiceTokenProvider _azureServiceTokenProvider;
        private readonly IDataCollectionsResponseRepository _repository;

        public LearnerService(
            ILogger<ILearnerService> logger,
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

        private async Task<LearnerNiNumber> SendIndividualRequest(Models.EmploymentCheck employmentCheck, AuthResult token)
        {
            // Setup config for DataCollections Api Call
            HttpClient client;
            string url;
            SetupDataCollectionsApitConfig(employmentCheck, token, out client, out url);

            // Call the DataCollections Api to get the Learner Ni Number
            var learnerNiNumber = await ExecuteDataCollectionsApiCall(employmentCheck, client, url);

            // Return the learner Ni Number to the caller
            return learnerNiNumber ?? new LearnerNiNumber();
        }

        private void SetupDataCollectionsApitConfig(Models.EmploymentCheck employmentCheck, AuthResult token, out HttpClient client, out string url)
        {
            // Setup config for Data Collections Api call
            client = _httpFactory.CreateClient("LearnerNiApi");
            client.BaseAddress = new Uri(_dcApiSettings.BaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            url = LearnersNiUrl + employmentCheck.Uln;
        }

        private async Task<LearnerNiNumber> ExecuteDataCollectionsApiCall(Models.EmploymentCheck employmentCheck, HttpClient client, string url)
        {
            LearnerNiNumber learnerNiNumber = null;
            try
            {
                // Call the Data Collections Api
                var response = await client.GetAsync(url);

                // Get the NiNumber from the response
                learnerNiNumber = await GetApiResponseNiNumber(employmentCheck, response);
            }
            catch (Exception e)
            {
                await HandleException(employmentCheck, e);
            }

            // Return the LearnerNiNumber
            return learnerNiNumber ?? new LearnerNiNumber();
        }

        private async Task<LearnerNiNumber> GetApiResponseNiNumber(
            Models.EmploymentCheck employmentCheck,
            HttpResponseMessage httpResponseMessage
        )
        {
            // This has already been null checked earlier in the call-chain so should not be null
            Guard.Against.Null(employmentCheck, nameof(employmentCheck));

            // Create a 'DataCollectionsResponse' model to hold the api response data to store in the database
            var dataCollectionsResponse = await InitialiseDataCollectionsResponseModel(employmentCheck);

            // Check we have response data
            if (httpResponseMessage == null)
            {
                await Save(dataCollectionsResponse);
                return await Task.FromResult(new LearnerNiNumber());
            }

            // Store the api response data in the DataCollectionsResponse model
            dataCollectionsResponse.HttpResponse = httpResponseMessage.ToString();
            dataCollectionsResponse.HttpStatusCode = (short)httpResponseMessage.StatusCode;

            // Check the response success status code to determine if the api call succeeded
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                // The call wasn't successful, save the api response data and return an empty LearnerNiNumber
                await Save(dataCollectionsResponse);
                return await Task.FromResult(new LearnerNiNumber());
            }

            // The api call was succesful, read the response content
            var jsonContent = await ReadResponseContent(httpResponseMessage, dataCollectionsResponse);

            // Deserialise the LearnerNiNumber from the content
            var learnerNiNumber = await DeserialiseContent(jsonContent, dataCollectionsResponse);

            // return the employer schemes to the caller
            return learnerNiNumber;
        }

        private async Task<DataCollectionsResponse> InitialiseDataCollectionsResponseModel(Models.EmploymentCheck employmentCheck)
        {
            // Create a 'DataCollectionsResponse' model to hold the api response data to store in the database
            return await Task.FromResult(new DataCollectionsResponse(
                employmentCheck.Id,
                employmentCheck.CorrelationId,
                employmentCheck.Uln,
                string.Empty,                                   // NiNumber
                string.Empty,                                   // Response
                (short)HttpStatusCode.InternalServerError));    // Http Status Code
        }

        private async Task<Stream> ReadResponseContent(
            HttpResponseMessage httpResponseMessage,
            DataCollectionsResponse dataCollectionsResponse
        )
        {
            // This has already been null checked earlier in the call-chain so should not be null
            Guard.Against.Null(dataCollectionsResponse, nameof(dataCollectionsResponse));

            // This has already been null checked earlier in the call-chain so should not be null
            Guard.Against.Null(httpResponseMessage, nameof(httpResponseMessage));

            // Read the content
            var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
            if ((stream == null || stream.Length == 0))
            {
                // Nothing to read, store the DataCollectionsResponse model and return null
                await Save(dataCollectionsResponse);
                return null;
            }

            return stream;
        }

        private async Task<LearnerNiNumber> DeserialiseContent(
            Stream stream,
            DataCollectionsResponse dataCollectionsResponse
        )
        {
            // Check the jsonContent has a value
            if (stream == null)
            {
                await Save(dataCollectionsResponse);
                return await Task.FromResult(new LearnerNiNumber());
            }

            // This has already been null checked earlier in the call-chain so should not be null
            Guard.Against.Null(dataCollectionsResponse, nameof(dataCollectionsResponse));

            // Deserialise the json content
            var learnerNiNumbers = await JsonSerializer.DeserializeAsync<List<LearnerNiNumber>>(stream);
            if (learnerNiNumbers == null)
            {
                // Nothing to deserialise, store the DataCollectionsResponse and return an empty LearnerNiNumber
                await Save(dataCollectionsResponse);
                return await Task.FromResult(new LearnerNiNumber());
            }

            var learnerNiNumber = learnerNiNumbers.FirstOrDefault();

            // store the Ni Number in the DataCollections model and save the model to the database
            dataCollectionsResponse.NiNumber = learnerNiNumber?.NiNumber;
            await Save(dataCollectionsResponse);

            // Return the employer paye schemes to the caller
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

        private async Task Save(DataCollectionsResponse dataCollectionsResponse)
        {
            if (dataCollectionsResponse == null)
            {
                _logger.LogError($"LearnerService.Save(): ERROR: The dataCollectionsResponse model is null.");
                return;
            }

            // Temporary work-around try/catch for handling duplicate inserts until we switch to single message processing
            try
            {
                await _repository.Save(dataCollectionsResponse);
            }
            catch
            {
                // No logging, we're not interested in storing errors about duplicates at the moment
            }
        }

        private async Task HandleException(Models.EmploymentCheck employmentCheck, Exception e)
        {
            var dataCollectionsResponse = await InitialiseDataCollectionsResponseModel(employmentCheck);

            dataCollectionsResponse.HttpResponse = e.Message;
            await Save(dataCollectionsResponse);
        }
    }
}