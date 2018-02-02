using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmploymentCheck.Application.Gateways;
using SFA.DAS.EmploymentCheck.Application.Services;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmploymentCheck.Application.Commands.PerformEmploymentCheck
{
    public class PerformEmploymentCheckCommand : IAsyncNotificationHandler<PerformEmploymentCheckRequest>
    {
        private readonly IHmrcGateway _hmrcGateway;
        private readonly EmploymentCheckCompletedService _employmentCheckService;
        private readonly ILog _logger;

        public PerformEmploymentCheckCommand(IHmrcGateway hmrcGateway, IEventsApi eventsApi, ISubmissionEventRepository repository, ILog logger)
        {
            _hmrcGateway = hmrcGateway;
            _employmentCheckService = new EmploymentCheckCompletedService(eventsApi, repository);
            _logger = logger;
        }

        public async Task Handle(PerformEmploymentCheckRequest notification)
        {
            try
            {
                _logger.Info($"Performing employment check for {notification.NationalInsuranceNumber}");

                var checkPassed = await DoEmploymentCheck(notification);

                _logger.Info($"Employment check completed for {notification.NationalInsuranceNumber}, result = {checkPassed}");

                await _employmentCheckService.CompleteEmploymentCheck(notification.NationalInsuranceNumber, notification.Uln, notification.EmployerAccountId, notification.Ukprn, checkPassed);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error performing employment check - {ex.Message}");
                throw;
            }
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
