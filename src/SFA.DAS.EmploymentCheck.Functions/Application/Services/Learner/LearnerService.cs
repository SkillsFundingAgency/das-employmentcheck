﻿using Ardalis.GuardClauses;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Collections.Generic;
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

        public async Task<IList<LearnerNiNumber>> GetDbNiNumbers(IList<Models.EmploymentCheck> employmentCheckBatch)
        {
            var learnerNiNumbers = new List<LearnerNiNumber>();
            foreach (var employmentCheck in employmentCheckBatch)
            {
                LearnerNiNumber learnerNiNumber = null;
                var response = await _repository.GetByEmploymentCheckId(employmentCheck.Id);
                if (response != null)
                {
                    learnerNiNumber = new LearnerNiNumber { Uln = employmentCheck.Uln, NiNumber = response.NiNumber };
                    learnerNiNumbers.Add(learnerNiNumber);
                }
            }

            return learnerNiNumbers;
        }

        public async Task<IList<LearnerNiNumber>> GetNiNumbers(IList<Models.EmploymentCheck> employmentCheckBatch)
        {
            Guard.Against.NullOrEmpty(employmentCheckBatch, nameof(employmentCheckBatch));

            var token = GetDcToken().Result;
            var learnerNiNumbers = await GetNiNumbers(employmentCheckBatch, token);

            return await Task.FromResult(learnerNiNumbers ?? new List<LearnerNiNumber>());
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
            HttpClient client;
            string url;
            SetupDataCollectionsApitConfig(employmentCheck, token, out client, out url);

            var learnerNiNumber = await ExecuteDataCollectionsApiCall(employmentCheck, client, url);

            return learnerNiNumber ?? new LearnerNiNumber();
        }

        private void SetupDataCollectionsApitConfig(Models.EmploymentCheck employmentCheck, AuthResult token, out HttpClient client, out string url)
        {
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
                var response = await client.GetAsync(url);
                learnerNiNumber = await GetNiNumberFromApiResponse(employmentCheck, response);
            }
            catch (Exception e)
            {
                await HandleException(employmentCheck, e);
            }

            return learnerNiNumber ?? new LearnerNiNumber();
        }

        private async Task<LearnerNiNumber> GetNiNumberFromApiResponse(
            Models.EmploymentCheck employmentCheck,
            HttpResponseMessage httpResponseMessage
        )
        {
            Guard.Against.Null(employmentCheck, nameof(employmentCheck));

            var dataCollectionsResponse = await InitialiseDataCollectionsResponseModel(employmentCheck);
            if (httpResponseMessage == null)
            {
                await Save(dataCollectionsResponse);
                return await Task.FromResult(new LearnerNiNumber());
            }

            dataCollectionsResponse.HttpResponse = httpResponseMessage.ToString();
            dataCollectionsResponse.HttpStatusCode = (short)httpResponseMessage.StatusCode;

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                await Save(dataCollectionsResponse);
                return await Task.FromResult(new LearnerNiNumber());
            }

            var jsonContent = await ReadResponseContent(httpResponseMessage, dataCollectionsResponse);
            var learnerNiNumber = await DeserialiseContent(jsonContent, dataCollectionsResponse);

            return learnerNiNumber;
        }

        private static async Task<DataCollectionsResponse> InitialiseDataCollectionsResponseModel(Models.EmploymentCheck employmentCheck)
        {
            return await Task.FromResult(new DataCollectionsResponse(
                    0,
                    employmentCheck.Id,
                    employmentCheck.CorrelationId,
                    employmentCheck.Uln,
                    string.Empty, // NiNumber
                    string.Empty, // Response
                    (short)HttpStatusCode.InternalServerError // Http Status Code
                )
            );
        }

        private async Task<Stream> ReadResponseContent(
            HttpResponseMessage httpResponseMessage,
            DataCollectionsResponse dataCollectionsResponse
        )
        {
            Guard.Against.Null(dataCollectionsResponse, nameof(dataCollectionsResponse));
            Guard.Against.Null(httpResponseMessage, nameof(httpResponseMessage));

            var stream = await httpResponseMessage.Content.ReadAsStreamAsync();
            if ((stream == null || stream.Length == 0))
            {
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
            if (stream == null)
            {
                await Save(dataCollectionsResponse);
                return await Task.FromResult(new LearnerNiNumber());
            }

            Guard.Against.Null(dataCollectionsResponse, nameof(dataCollectionsResponse));

            var learnerNiNumbers = await JsonSerializer.DeserializeAsync<List<LearnerNiNumber>>(stream);
            if (learnerNiNumbers == null)
            {
                await Save(dataCollectionsResponse);
                return await Task.FromResult(new LearnerNiNumber());
            }

            var learnerNiNumber = learnerNiNumbers.FirstOrDefault();
            dataCollectionsResponse.NiNumber = learnerNiNumber?.NiNumber;
            await Save(dataCollectionsResponse);

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

            await _repository.InsertOrUpdate(dataCollectionsResponse);
        }

        private async Task HandleException(Models.EmploymentCheck employmentCheck, Exception e)
        {
            var dataCollectionsResponse = await InitialiseDataCollectionsResponseModel(employmentCheck);

            dataCollectionsResponse.HttpResponse = e.Message;
            await Save(dataCollectionsResponse);
        }
    }
}