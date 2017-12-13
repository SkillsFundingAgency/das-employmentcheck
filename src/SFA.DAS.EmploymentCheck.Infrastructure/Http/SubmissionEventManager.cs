using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.EmploymentCheck.Application.Models;
using SFA.DAS.EmploymentCheck.Application.Orchestrators;
using SFA.DAS.EmploymentCheck.Domain;
using SFA.DAS.EmploymentCheck.Domain.Configuration;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Models;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Http
{
    public class SubmissionEventManager : ISubmissionEventManager
    {
        private readonly IHttpClientWrapper _httpClientWrapper;
        private readonly ISubmissionEventOrchestrator _orchestrator;
        private readonly IEmploymentCheckConfiguration _configuration;

        public SubmissionEventManager(IHttpClientWrapper httpClientWrapper, ISubmissionEventOrchestrator orchestrator,
            IEmploymentCheckConfiguration configuration)
        {
            if (httpClientWrapper == null)
            {
                throw new ArgumentNullException(nameof(httpClientWrapper), "Ensure that the ioc mapping is registered for the httpClientWrapper");
            }

            if (orchestrator == null)
            {
                throw new ArgumentNullException(nameof(orchestrator), "Ensure that the ioc mapping is registered for the orchestrator");
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(orchestrator), "Ensure that the configuration mapping is registered for the application");
            }

            _httpClientWrapper = httpClientWrapper;
            _orchestrator = orchestrator;
            _configuration = configuration;
        }

        public long LastProcessedEventId { get; private set; }

        public async Task PollSubmissionEvents()
        {
            var unProcessedEvents = await GetSubmissionEventsToProcess();
            var previouslyHandledEvents = await GetPreviouslyHandledEvents(unProcessedEvents);
            var itemsToProcess = RemoveSuccessfullyHandledEventsFrom(previouslyHandledEvents, unProcessedEvents);

            // the above line removes submission events that we have already processed successfully previously
            // I think that we may require another trimming of the itemsToProcess list with nino numbers that have changed but have been handled succesfully, 
            // unsure of how we would match this just yet though, is the uln sufficient enough or do we need to use ukprn or some other field to be able to do this
            // all that will happen is that the incorrect nino number will fail each time the hmrc check is performed as it will keep getting added the the messagebus

            foreach (var submissionEvent in itemsToProcess)
            {
                // add each item to the messagebus

                LastProcessedEventId = submissionEvent.Id;
            }
        }

        private IEnumerable<NinoChangedEventMessage> RemoveSuccessfullyHandledEventsFrom(OrchestratorResponse<PreviouslyHandledSubmissionEventViewModel> previouslyHandledEvents,
            PageOfResults<NinoChangedEventMessage> unProcessedEvents)
        {
            var previouslyHandledItemsToRemoveFromProcessing =
                ExcludePreviouslySuccessfulUlnAndNinoMatchesFromListOfEventsToProcess(previouslyHandledEvents.Data.Events,
                    unProcessedEvents.Items);

            var itemsToProcess = unProcessedEvents.Items.Except(previouslyHandledItemsToRemoveFromProcessing);
            return itemsToProcess;
        }

        private async Task<OrchestratorResponse<PreviouslyHandledSubmissionEventViewModel>> GetPreviouslyHandledEvents(PageOfResults<NinoChangedEventMessage> unProcessedEvents)
        {
            var ulns = unProcessedEvents.Items.Select(submissionEvent => submissionEvent.Uln).Distinct().ToList();
            var previouslyHandledEvents =
                await _orchestrator.GetPreviouslyHandledSubmissionEvents(JsonConvert.SerializeObject(ulns));
            return previouslyHandledEvents;
        }

        private async Task<PageOfResults<NinoChangedEventMessage>> GetSubmissionEventsToProcess()
        {
            var url = string.Format(CultureInfo.InvariantCulture, _configuration.SubmissionEventApiAddress,
                LastProcessedEventId);

            var submissionEventResponse = await _httpClientWrapper.GetAsync(url.ToUri());
            var unProcessedEvents = await _httpClientWrapper.ReadResponse<PageOfResults<NinoChangedEventMessage>>(submissionEventResponse);
            return unProcessedEvents;
        }

        private IEnumerable<NinoChangedEventMessage> ExcludePreviouslySuccessfulUlnAndNinoMatchesFromListOfEventsToProcess(
            IEnumerable<PreviousHandledSubmissionEvent> previouslyHandled, NinoChangedEventMessage[] unProcessedItems)
        {
            var eventListItemsToRemove = new List<NinoChangedEventMessage>();
            
            foreach (var previousHandledSubmissionEvent in previouslyHandled)
            {
                eventListItemsToRemove.AddRange(unProcessedItems.Where(unProcessed => 
                string.Compare(unProcessed.Uln, previousHandledSubmissionEvent.Uln, StringComparison.OrdinalIgnoreCase) == 0 && 
                string.Compare(unProcessed.NiNumber, previousHandledSubmissionEvent.NiNumber, StringComparison.OrdinalIgnoreCase) == 0 
                && previousHandledSubmissionEvent.PassedValidationCheck));
            }

            return eventListItemsToRemove;
        }

        public async Task DetermineProcessingStartingPoint()
        {
            var response = await _orchestrator.GetLastKnownProcessedSubmissionEventId();

            if (response.Data > 0)
            {
                LastProcessedEventId = response.Data;
            }
        }
    }
}
