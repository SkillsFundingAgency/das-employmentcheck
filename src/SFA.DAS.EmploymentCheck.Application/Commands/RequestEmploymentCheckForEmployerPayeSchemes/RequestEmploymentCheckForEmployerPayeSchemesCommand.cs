using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmploymentCheck.Events;
using SFA.DAS.Messaging.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmploymentCheck.Application.Commands.RequestEmploymentCheckForEmployerPayeSchemes
{
    public class RequestEmploymentCheckForEmployerPayeSchemesCommand : IAsyncNotificationHandler<RequestEmploymentCheckForEmployerPayeSchemesRequest>
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IAccountApiClient _accountApiClient;
        private readonly IProviderCommitmentsApi _commitmentsApi;
        private readonly ILog _logger;

        public RequestEmploymentCheckForEmployerPayeSchemesCommand(IMessagePublisher messagePublisher, IAccountApiClient accountApiClient, IProviderCommitmentsApi commitmentsApi, ILog logger)
        {
            _messagePublisher = messagePublisher;
            _accountApiClient = accountApiClient;
            _commitmentsApi = commitmentsApi;
            _logger = logger;
        }

        public async Task Handle(RequestEmploymentCheckForEmployerPayeSchemesRequest notification)
        {
            try
            {
                var employerAccountId = await GetEmployerAccountId(notification);
                var payeSchemes = await GetPayeSchemesForAccount(employerAccountId);

                await RequestEmploymentCheck(notification, employerAccountId, payeSchemes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error getting paye schemes for employment check - {ex.Message}");
                throw;
            }
        }

        private async Task<long> GetEmployerAccountId(RequestEmploymentCheckForEmployerPayeSchemesRequest notification)
        {
            _logger.Info($"Getting PAYE schemes for apprenticeship {notification.ApprenticeshipId} to perform employment check for NINO: {notification.NationalInsuranceNumber}");
            var apprenticeship = await _commitmentsApi.GetProviderApprenticeship(notification.Ukprn, notification.ApprenticeshipId);
            return apprenticeship.EmployerAccountId;
        }

        private async Task RequestEmploymentCheck(RequestEmploymentCheckForEmployerPayeSchemesRequest notification, long employerAccountId, IEnumerable<string> payeSchemes)
        {
            await _messagePublisher.PublishAsync(new EmploymentCheckRequiredForAccountMessage(
                notification.NationalInsuranceNumber, notification.Uln, employerAccountId, notification.Ukprn,
                notification.ActualStartDate, payeSchemes));
        }

        private async Task<IEnumerable<string>> GetPayeSchemesForAccount(long employerAccountId)
        {
            _logger.Info($"Getting PAYE schemes for account {employerAccountId} to perform employment check");
            var account = await _accountApiClient.GetAccount(employerAccountId);
            var payeSchemes = account.PayeSchemes.Select(x => x.Id);
            return payeSchemes;
        }
    }
}
