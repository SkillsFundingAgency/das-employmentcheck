using Ardalis.GuardClauses;
using Newtonsoft.Json;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber
{
    public class NationalInsuranceNumberService : INationalInsuranceNumberService
    {
        private readonly IApiRetryPolicies _apiRetryPolicies;
        private readonly IDataCollectionsApiClient<DataCollectionsApiConfiguration> _apiClient;
        private readonly DataCollectionsApiConfiguration _apiConfiguration;
        private readonly IDataCollectionsResponseRepository _repository;

        public NationalInsuranceNumberService(
            IApiRetryPolicies apiRetryPolicies,
            IDataCollectionsApiClient<DataCollectionsApiConfiguration> apiClient,
            DataCollectionsApiConfiguration apiConfiguration,
            IDataCollectionsResponseRepository repository)
        {
            _apiClient = apiClient;
            _apiRetryPolicies = apiRetryPolicies;
            _apiConfiguration = apiConfiguration;
            _repository = repository;
        }

        public async Task<LearnerNiNumber> Get(NationalInsuranceNumberRequest nationalInsuranceNumberRequest)
        {
            var policy = await _apiRetryPolicies.GetAll("LearnerApiKey");
            var request = new GetNationalInsuranceNumberRequest(nationalInsuranceNumberRequest.EmploymentCheck.Uln, nationalInsuranceNumberRequest.AcademicYear, _apiConfiguration);
            var response = await _apiClient.GetWithPolicy(policy, request);
            return await ProcessNiNumberFromApiResponse(nationalInsuranceNumberRequest.EmploymentCheck, response);
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
                if (learnerNiNumbers != null)
                {
                    var learnerNiNumber = learnerNiNumbers.FirstOrDefault();
                    learnerNiNumber.HttpStatusCode = (HttpStatusCode)dataCollectionsResponse.HttpStatusCode;
                    return learnerNiNumber;
                }
            }

            return new LearnerNiNumber(dataCollectionsResponse.Uln, null, (HttpStatusCode)dataCollectionsResponse.HttpStatusCode);
        }

        private async Task Save(DataCollectionsResponse dataCollectionsResponse)
        {
            await _repository.InsertOrUpdate(dataCollectionsResponse);
        }
    }

}
