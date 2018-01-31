using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Models;
using SFA.DAS.EmploymentCheck.Events;
using SFA.DAS.Messaging.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Provider.Events.Api.Client;
using SFA.DAS.Provider.Events.Api.Types;

namespace SFA.DAS.EmploymentCheck.Application.Commands.InitiateEmploymentCheckForChangedNationalInsuranceNumbers
{
    public class InitiateEmploymentCheckForChangedNationalInsuranceNumbersCommand : IAsyncNotificationHandler<InitiateEmploymentCheckForChangedNationalInsuranceNumbersRequest>
    {
        private ISubmissionEventRepository _repository;
        private readonly IPaymentsEventsApiClient _eventsApi;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILog _logger;

        public InitiateEmploymentCheckForChangedNationalInsuranceNumbersCommand(ISubmissionEventRepository repository, IPaymentsEventsApiClient eventsApi, IMessagePublisher messagePublisher, ILog logger)
        {
            _repository = repository;
            _eventsApi = eventsApi;
            _messagePublisher = messagePublisher;
            _logger = logger;
        }

        public async Task Handle(InitiateEmploymentCheckForChangedNationalInsuranceNumbersRequest notification)
        {
            try
            {
                var unprocessedEvents = await GetUnprocessedEvents();

                _logger.Info($"{unprocessedEvents.Count()} unprocessed events to be processed");

                var previousEmploymentCheckResults = await GetPreviousEmploymentCheckResults(unprocessedEvents);

                var eventsRequiringEmploymentCheck = GetEventsRequiringEmploymentCheck(unprocessedEvents, previousEmploymentCheckResults);

                _logger.Info($"{eventsRequiringEmploymentCheck.Count()} events require employment check");

                await RequestEmploymentCheck(eventsRequiringEmploymentCheck);

                _logger.Info($"Employment check requested for {eventsRequiringEmploymentCheck.Count()} events");

                await StoreLastProcessedEvent(unprocessedEvents);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error processing submission events - {ex.Message}");
            }
        }

        private async Task StoreLastProcessedEvent(IEnumerable<SubmissionEvent> unprocessedEvents)
        {
            if (unprocessedEvents.Any())
            {
                await _repository.SetLastProcessedEvent(unprocessedEvents.Last().Id);

                _logger.Info($"Last processed event id set to {unprocessedEvents.Last().Id}");
            }
        }

        private async Task RequestEmploymentCheck(IEnumerable<SubmissionEvent> eventsRequiringEmploymentCheck)
        {
            var publishTasks = eventsRequiringEmploymentCheck.Select(x =>
                _messagePublisher.PublishAsync(new EmploymentCheckRequiredForApprenticeMessage(x.NiNumber, x.Uln,
                    x.ApprenticeshipId.Value, x.Ukprn, x.ActualStartDate.Value)));
            await Task.WhenAll(publishTasks);
        }

        private IEnumerable<SubmissionEvent> GetEventsRequiringEmploymentCheck(IEnumerable<SubmissionEvent> unprocessedEvents, IEnumerable<PreviousHandledSubmissionEvent> previousEmploymentCheckResults)
        {
            var eventsWithSufficientData = GetEventsWithMandatoryEmploymentCheckData(unprocessedEvents);
            return eventsWithSufficientData.Where(submissionEvent => !previousEmploymentCheckResults.Any(x => x.Uln == submissionEvent.Uln && x.NiNumber == submissionEvent.NiNumber && x.PassedValidationCheck)).ToList();
        }

        private IEnumerable<SubmissionEvent> GetEventsWithMandatoryEmploymentCheckData(IEnumerable<SubmissionEvent> unprocessedEvents)
        {
            return unprocessedEvents.Where(x => x.ActualStartDate.HasValue && x.ApprenticeshipId.HasValue);
        }

        private async Task<IEnumerable<PreviousHandledSubmissionEvent>> GetPreviousEmploymentCheckResults(IEnumerable<SubmissionEvent> unprocessedEvents)
        {
            var ulns = unprocessedEvents.Select(x => x.Uln).Distinct();
            var previousResults = await _repository.GetPreviouslyHandledSubmissionEvents(ulns);
            return previousResults;
        }

        private async Task<IEnumerable<SubmissionEvent>> GetUnprocessedEvents()
        {
            var startingEventId = await GetStartingEventId();
            var unprocessedEvents = await _eventsApi.GetSubmissionEvents(startingEventId);
            return unprocessedEvents.Items;
        }

        private async Task<long> GetStartingEventId()
        {
            var lastProcessedEventId = await _repository.GetLastProcessedEventId();
            var startingEventId = lastProcessedEventId;
            return startingEventId;
        }
    }
}
