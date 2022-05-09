using Ardalis.GuardClauses;
using Boxed.AspNetCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Services.Learner
{
    public class LearnerService : ILearnerService
    {
        private readonly IDataCollectionsApiClient<DataCollectionsApiConfiguration> _apiClient;
        private readonly IDataCollectionsResponseRepository _repository;
        private readonly IApiRetryPolicies _apiRetryPolicies;
        private readonly ILogger<LearnerService> _logger;

        public LearnerService(
            IDataCollectionsApiClient<DataCollectionsApiConfiguration> apiClient,
            IDataCollectionsResponseRepository repository,
            IApiRetryPolicies apiRetryPolicies,
            ILogger<LearnerService> logger
        )
        {
            _apiClient = apiClient;
            _repository = repository;
            _apiRetryPolicies = apiRetryPolicies;
            _logger = logger;
        }

        public async Task<LearnerNiNumber> GetDbNiNumber(Data.Models.EmploymentCheck employmentCheck)
        {
            var response = await _repository.GetByEmploymentCheckId(employmentCheck.Id);
            if (response != null && response.NiNumber != null)
            {
                return new LearnerNiNumber(employmentCheck.Uln, response.NiNumber, HttpStatusCode.OK);
            }

            return null;
        }

        public async Task<LearnerNiNumber> GetNiNumber(Data.Models.EmploymentCheck employmentCheck)
        {
            HttpResponseMessage response = null;

            try
            {
                var policy = await _apiRetryPolicies.GetAll();
                var request = new GetNationalInsuranceNumberRequest(employmentCheck.Uln);
                
                await policy.ExecuteAsync(async () =>
                {
                    _logger.LogInformation($"{nameof(LearnerService)}: Refreshing access token...");
                    response = await _apiClient.Get(request);
                    if (response != null && !response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"{nameof(LearnerService)}: Throwing exception for retry");
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                            throw new UnauthorizedAccessException();

                        throw new HttpException(response.StatusCode);
                    }

                });

                return await ProcessNiNumberFromApiResponse(employmentCheck, response);

            }
            catch (HttpException)
            {
                return await ProcessNiNumberFromApiResponse(employmentCheck, response);
            }
            catch (UnauthorizedAccessException)
            {
                return await ProcessNiNumberFromApiResponse(employmentCheck, response);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(LearnerService)}: Exception occurred [{e}]");

                await HandleException(employmentCheck, e);
                return null;
            }
        }

        private async Task<LearnerNiNumber> ProcessNiNumberFromApiResponse(Data.Models.EmploymentCheck employmentCheck, HttpResponseMessage httpResponseMessage)
        {
            Guard.Against.Null(employmentCheck, nameof(employmentCheck));

            if (httpResponseMessage == null)
            {
                await Save(CreateResponseModel(employmentCheck));
                return null;
            }

            var response = CreateResponseModel(employmentCheck, httpResponseMessage.ToString(), httpResponseMessage.StatusCode);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                await Save(response);
            }
            
            var jsonContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            try
            {
                var learnerNiNumber = DeserialiseContent(jsonContent, response);

                response.SetNiNumber(learnerNiNumber?.NiNumber);

                await Save(response);

                return learnerNiNumber;
            }
            catch (Exception)
            {
                return new LearnerNiNumber(response.Uln, response.NiNumber, (HttpStatusCode)response.HttpStatusCode);
            }
        }

        private static DataCollectionsResponse CreateResponseModel(Data.Models.EmploymentCheck employmentCheck, string httpResponseMessage = null, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            return DataCollectionsResponse.CreateResponse(
                employmentCheck.Id,
                employmentCheck.CorrelationId,
                employmentCheck.Uln,
                httpResponseMessage,
                (short)statusCode);
        }

        private static LearnerNiNumber DeserialiseContent(string jsonContent, DataCollectionsResponse dataCollectionsResponse)
        {
            Guard.Against.Null(dataCollectionsResponse, nameof(dataCollectionsResponse));

            if (!string.IsNullOrEmpty(jsonContent))
            {
                var learnerNiNumbers = JsonConvert.DeserializeObject<List<LearnerNiNumber>>(jsonContent);
                if(learnerNiNumbers != null)
                {
                    var learnerNiNumber = learnerNiNumbers.FirstOrDefault();
                    learnerNiNumber.HttpStatusCode = (HttpStatusCode)dataCollectionsResponse.HttpStatusCode;
                    return learnerNiNumber;
                }
            }

            return new LearnerNiNumber(dataCollectionsResponse.Uln, null, (HttpStatusCode)dataCollectionsResponse.HttpStatusCode);
        }

        private async Task HandleException(Data.Models.EmploymentCheck employmentCheck, Exception e)
        {
            var dataCollectionsResponse = CreateResponseModel(employmentCheck, e.Message);

            await Save(dataCollectionsResponse);
        }

        private async Task Save(DataCollectionsResponse dataCollectionsResponse)
        {
            await _repository.InsertOrUpdate(dataCollectionsResponse);
        }
    }
}