using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
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
        private readonly ILog _logger;

        public RequestEmploymentCheckForEmployerPayeSchemesCommand(IMessagePublisher messagePublisher, IAccountApiClient accountApiClient, ILog logger)
        {
            _messagePublisher = messagePublisher;
            _accountApiClient = accountApiClient;
            _logger = logger;
        }

        public async Task Handle(RequestEmploymentCheckForEmployerPayeSchemesRequest notification)
        {
            try
            {
                _logger.Info($"Getting PAYE schemes for account {notification.EmployerAccountId} to perform employment check for NINO: {notification.NationalInsuranceNumber}");
                var payeSchemes = await GetPayeSchemesForAccount(notification);

                await RequestEmploymentCheck(notification, payeSchemes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error getting paye schemes for employment check - {ex.Message}");
                throw;
            }
        }

        private async Task RequestEmploymentCheck(RequestEmploymentCheckForEmployerPayeSchemesRequest notification, IEnumerable<string> payeSchemes)
        {
            await _messagePublisher.PublishAsync(new EmploymentCheckRequiredForAccountMessage(
                notification.NationalInsuranceNumber, notification.Uln, notification.EmployerAccountId, notification.Ukprn,
                notification.ActualStartDate, payeSchemes));
        }

        private async Task<IEnumerable<string>> GetPayeSchemesForAccount(RequestEmploymentCheckForEmployerPayeSchemesRequest notification)
        {
            var account = await _accountApiClient.GetAccount(notification.EmployerAccountId);
            var payeSchemes = account.PayeSchemes.Select(x => x.Id);
            return payeSchemes;
        }
    }
}
