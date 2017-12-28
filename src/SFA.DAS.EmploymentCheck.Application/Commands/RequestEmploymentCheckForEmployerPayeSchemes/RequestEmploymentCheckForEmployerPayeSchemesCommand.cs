using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmploymentCheck.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.EmploymentCheck.Application.Commands.RequestEmploymentCheckForEmployerPayeSchemes
{
    public class RequestEmploymentCheckForEmployerPayeSchemesCommand : IAsyncNotificationHandler<RequestEmploymentCheckForEmployerPayeSchemesRequest>
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IAccountApiClient _accountApiClient;

        public RequestEmploymentCheckForEmployerPayeSchemesCommand(IMessagePublisher messagePublisher, IAccountApiClient accountApiClient)
        {
            _messagePublisher = messagePublisher;
            _accountApiClient = accountApiClient;
        }

        public async Task Handle(RequestEmploymentCheckForEmployerPayeSchemesRequest notification)
        {
            var payeSchemes = await GetPayeSchemesForAccount(notification);

            await RequestEmploymentCheck(notification, payeSchemes);
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
