using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber
{
    public class NationalInsuranceNumberServiceWithLogging : INationalInsuranceNumberService
    {
        private readonly INationalInsuranceNumberService _nationalInsuranceNumberService;
        private readonly ILogger<LearnerService> _logger;
        private readonly IDataCollectionsResponseRepository _repository;

        public NationalInsuranceNumberServiceWithLogging(
            INationalInsuranceNumberService nationalInsuranceNumberService,
            ILogger<LearnerService> logger,
            IDataCollectionsResponseRepository repository
            )
        {
            _nationalInsuranceNumberService = nationalInsuranceNumberService;
            _logger = logger;
            _repository = repository;
        }

        public async Task<LearnerNiNumber> Get(NationalInsuranceNumberRequest nationalInsuranceNumberRequest)
        {
            try
            {
                return await _nationalInsuranceNumberService.Get(nationalInsuranceNumberRequest);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(LearnerService)}: Exception occurred [{e}]");
                await HandleException(nationalInsuranceNumberRequest.EmploymentCheck, e);
                return null;
            }
        }

        private async Task HandleException(Data.Models.EmploymentCheck employmentCheck, Exception e)
        {
            var dataCollectionsResponse = CreateResponseModel(employmentCheck, e.Message);

            await Save(dataCollectionsResponse);
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

        private async Task Save(DataCollectionsResponse dataCollectionsResponse)
        {
            await _repository.InsertOrUpdate(dataCollectionsResponse);
        }
    }
}
