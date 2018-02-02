using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmploymentCheck.Application.Services;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Models;
using SFA.DAS.EmploymentCheck.Events;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Messaging.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmploymentCheck.Application.Commands.RequestEmploymentCheckForEmployerPayeSchemes
{
    public class RequestEmploymentCheckForEmployerPayeSchemesCommand : IAsyncNotificationHandler<RequestEmploymentCheckForEmployerPayeSchemesRequest>
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IAccountApiClient _accountApiClient;
        private readonly IProviderCommitmentsApi _commitmentsApi;
        private readonly EmploymentCheckCompletedService _employmentCheckService;
        private readonly ILog _logger;

        public RequestEmploymentCheckForEmployerPayeSchemesCommand(IMessagePublisher messagePublisher, IAccountApiClient accountApiClient, IProviderCommitmentsApi commitmentsApi, ISubmissionEventRepository repository, IEventsApi eventsApi, ILog logger)
        {
            _messagePublisher = messagePublisher;
            _accountApiClient = accountApiClient;
            _commitmentsApi = commitmentsApi;
            _employmentCheckService = new EmploymentCheckCompletedService(eventsApi, repository);
            _logger = logger;
        }

        public async Task Handle(RequestEmploymentCheckForEmployerPayeSchemesRequest notification)
        {
            try
            {
                var employerAccountId = await GetEmployerAccountId(notification);
                if (!employerAccountId.HasValue)
                {
                    await CreateNegativeEmploymentCheckResult(notification);
                    return;
                }

                var payeSchemes = await GetPayeSchemesForAccount(employerAccountId.Value);

                await RequestEmploymentCheck(notification, employerAccountId.Value, payeSchemes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error getting paye schemes for employment check - {ex.Message}");
                throw;
            }
        }

        private async Task CreateNegativeEmploymentCheckResult(RequestEmploymentCheckForEmployerPayeSchemesRequest notification)
        {
            await _employmentCheckService.CompleteEmploymentCheck(notification.NationalInsuranceNumber, notification.Uln, notification.Ukprn, false);
        }

        private async Task<long?> GetEmployerAccountId(RequestEmploymentCheckForEmployerPayeSchemesRequest notification)
        {
            try
            {
                _logger.Info($"Getting PAYE schemes for apprenticeship {notification.ApprenticeshipId} to perform employment check for NINO: {notification.NationalInsuranceNumber}");
                var apprenticeship = await _commitmentsApi.GetProviderApprenticeship(notification.Ukprn, notification.ApprenticeshipId);
                return apprenticeship.EmployerAccountId;
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("401"))
            {
                _logger.Info($"Commitments API returned UNAUTHORISED for apprentieceship {notification.ApprenticeshipId} and provider {notification.Ukprn}");
                return null;
            }
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
