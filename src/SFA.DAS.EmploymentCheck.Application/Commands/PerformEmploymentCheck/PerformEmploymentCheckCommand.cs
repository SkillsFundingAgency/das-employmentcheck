﻿using System;
using System.Threading.Tasks;
using MediatR;
using NLog;
using SFA.DAS.EmploymentCheck.Application.Gateways;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Models;
using SFA.DAS.EmploymentCheck.Events;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Types;

namespace SFA.DAS.EmploymentCheck.Application.Commands.PerformEmploymentCheck
{
    public class PerformEmploymentCheckCommand : IAsyncNotificationHandler<PerformEmploymentCheckRequest>
    {
        private readonly IHmrcGateway _hmrcGateway;
        private readonly IEventsApi _eventsApi;
        private readonly ISubmissionEventRepository _repository;
        private readonly ILogger _logger;

        public PerformEmploymentCheckCommand(IHmrcGateway hmrcGateway, IEventsApi eventsApi, ISubmissionEventRepository repository, ILogger logger)
        {
            _hmrcGateway = hmrcGateway;
            _eventsApi = eventsApi;
            _repository = repository;
            _logger = logger;
        }

        public async Task Handle(PerformEmploymentCheckRequest notification)
        {
            _logger.Info($"Performing employment check for {notification.NationalInsuranceNumber}");

            var checkPassed = await DoEmploymentCheck(notification);

            _logger.Info($"Employment check completed for {notification.NationalInsuranceNumber}, result = {checkPassed}");

            await StoreEmploymentCheckResult(notification, checkPassed);
            await CreateEmploymentCheckCompleteEvent(notification, checkPassed);
        }

        private async Task CreateEmploymentCheckCompleteEvent(PerformEmploymentCheckRequest notification, bool checkPassed)
        {
            var completeEvent = new EmploymentCheckCompleteEvent(notification.NationalInsuranceNumber, notification.Uln, notification.EmployerAccountId, notification.Ukprn, DateTime.Now, checkPassed);
            var genericEvent = new GenericEvent<EmploymentCheckCompleteEvent> { CreatedOn = DateTime.Now, Payload = completeEvent };
            await _eventsApi.CreateGenericEvent<EmploymentCheckCompleteEvent>(genericEvent);
        }

        private async Task StoreEmploymentCheckResult(PerformEmploymentCheckRequest notification, bool checkPassed)
        {
            var employmentCheckResult = new PreviousHandledSubmissionEvent {Uln = notification.Uln, NiNumber = notification.NationalInsuranceNumber, PassedValidationCheck = checkPassed};
            await _repository.StoreEmploymentCheckResult(employmentCheckResult);
        }

        private async Task<bool> DoEmploymentCheck(PerformEmploymentCheckRequest notification)
        {
            var checkPassed = false;
            foreach (var payeScheme in notification.PayeSchemes)
            {
                _logger.Info($"Calling HMRC employment check for NINO: {notification.NationalInsuranceNumber} and Paye Scheme: {payeScheme}");

                checkPassed = await _hmrcGateway.IsNationalInsuranceNumberRelatedToPayeScheme(payeScheme, notification.NationalInsuranceNumber, notification.ActualStartDate);
                if (checkPassed)
                {
                    break;
                }
            }
            return checkPassed;
        }
    }
}
