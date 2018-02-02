using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Models;
using SFA.DAS.EmploymentCheck.Events;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Types;

namespace SFA.DAS.EmploymentCheck.Application.Services
{
    internal class EmploymentCheckCompletedService
    {
        private readonly IEventsApi _eventsApi;
        private readonly ISubmissionEventRepository _repository;

        internal EmploymentCheckCompletedService(IEventsApi eventsApi, ISubmissionEventRepository repository)
        {
            _eventsApi = eventsApi;
            _repository = repository;
        }

        internal async Task CompleteEmploymentCheck(string nationalInsuranceNumber, long uln, long ukprn, bool checkPassed)
        {
            await CompleteEmploymentCheck(nationalInsuranceNumber, uln, 0, ukprn, checkPassed);
        }

        internal async Task CompleteEmploymentCheck(string nationalInsuranceNumber, long uln, long employerAccountId, long ukprn, bool checkPassed)
        {
            await CreateEmploymentCheckCompleteEvent(nationalInsuranceNumber, uln, employerAccountId, ukprn, checkPassed);
            await StoreEmploymentCheckResult(uln, nationalInsuranceNumber, checkPassed);
        }

        private async Task CreateEmploymentCheckCompleteEvent(string nationalInsuranceNumber, long uln, long employerAccountId, long ukprn, bool checkPassed)
        {
            var completeEvent = new EmploymentCheckCompleteEvent(nationalInsuranceNumber, uln, employerAccountId, ukprn, DateTime.Now, checkPassed);
            var genericEvent = new GenericEvent { CreatedOn = DateTime.Now, Payload = JsonConvert.SerializeObject(completeEvent), Type = completeEvent.GetType().Name };
            await _eventsApi.CreateGenericEvent(genericEvent);
        }

        private async Task StoreEmploymentCheckResult(long uln, string nationalInsuranceNumber, bool checkPassed)
        {
            var employmentCheckResult = new PreviousHandledSubmissionEvent { Uln = uln, NiNumber = nationalInsuranceNumber, PassedValidationCheck = checkPassed };
            await _repository.StoreEmploymentCheckResult(employmentCheckResult);
        }
    }
}
