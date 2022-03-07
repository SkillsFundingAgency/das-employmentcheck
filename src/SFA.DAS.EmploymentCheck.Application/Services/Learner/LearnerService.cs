using Ardalis.GuardClauses;
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
        private readonly ILogger<ILearnerService> _logger;
        private readonly IDataCollectionsApiClient<DataCollectionsApiConfiguration> _apiClient;
        private readonly IDataCollectionsResponseRepository _repository;

        public LearnerService(
            ILogger<ILearnerService> logger,
            IDataCollectionsApiClient<DataCollectionsApiConfiguration> apiClient,
            IDataCollectionsResponseRepository repository
        )
        {
            _logger = logger;
            _apiClient = apiClient;
            _repository = repository;
        }

        public async Task<LearnerNiNumber> GetDbNiNumber(Data.Models.EmploymentCheck employmentCheck)
        {
            LearnerNiNumber learnerNiNumber = null;
            var response = await _repository.GetByEmploymentCheckId(employmentCheck.Id);
            if (response != null && response.NiNumber != null)
            {
                learnerNiNumber = new LearnerNiNumber { Uln = employmentCheck.Uln, NiNumber = response.NiNumber };
            }

            return learnerNiNumber ?? new LearnerNiNumber();
        }

        public async Task<LearnerNiNumber> GetNiNumber(Data.Models.EmploymentCheck employmentCheck)
        {
            try
            {
                var request = new GetNationalInsuranceNumberRequest(employmentCheck.Uln);
                var response = await _apiClient.Get(request);
                
                return await ProcessNiNumberFromApiResponse(employmentCheck, response);
            }
            catch (Exception e)
            {
                await HandleException(employmentCheck, e);
            }

            return new LearnerNiNumber();
        }

        private async Task<LearnerNiNumber> ProcessNiNumberFromApiResponse(Data.Models.EmploymentCheck employmentCheck, HttpResponseMessage httpResponseMessage)
        {
            Guard.Against.Null(employmentCheck, nameof(employmentCheck));

            if (httpResponseMessage == null)
            {
                await Save(CreateResponseModel(employmentCheck));
                return new LearnerNiNumber();
            }

            var response = CreateResponseModel(employmentCheck, httpResponseMessage.ToString(), httpResponseMessage.StatusCode);

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                await Save(response);
                return new LearnerNiNumber();
            }

            var jsonContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            var learnerNiNumber = DeserialiseContent(jsonContent, response);
            
            response.SetNiNumber(learnerNiNumber.NiNumber);
            
            await Save(response);

            return learnerNiNumber;
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
                if (learnerNiNumbers != null)
                {
                    return learnerNiNumbers.FirstOrDefault();
                }
            }

            return new LearnerNiNumber(dataCollectionsResponse.Uln, string.Empty);
        }

        private async Task HandleException(Data.Models.EmploymentCheck employmentCheck, Exception e)
        {
            var dataCollectionsResponse = CreateResponseModel(employmentCheck, e.Message);

            await Save(dataCollectionsResponse);
        }

        private async Task Save(DataCollectionsResponse dataCollectionsResponse)
        {
            if (dataCollectionsResponse == null)
            {
                _logger.LogError($"{nameof(LearnerService)}.Save: ERROR: The dataCollectionsResponse model is null.");
                return;
            }

            await _repository.InsertOrUpdate(dataCollectionsResponse);
        }
    }
}