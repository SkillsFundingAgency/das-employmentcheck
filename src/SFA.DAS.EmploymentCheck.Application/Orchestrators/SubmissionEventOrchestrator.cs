using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmploymentCheck.Application.Queries.GetLastKnownProcessedSubmissionEvent;
using SFA.DAS.EmploymentCheck.Application.Queries.GetPreviouslyHandledSubmissionEventsFor;
using SFA.DAS.EmploymentCheck.Domain;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Models;

namespace SFA.DAS.EmploymentCheck.Application.Orchestrators
{
    public class SubmissionEventOrchestrator : ISubmissionEventOrchestrator
    {
        private readonly IMediator _mediator;

        public SubmissionEventOrchestrator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<OrchestratorResponse<long>> GetLastKnownProcessedSubmissionEventId()
        {
            var beginProcessingFrom = await _mediator.SendAsync(new GetLastKnownProcessedSubmissionEventRequest());

            return new OrchestratorResponse<long>(){Data = beginProcessingFrom.EventId};
        }

        public async Task<OrchestratorResponse<PreviouslyHandledSubmissionEventViewModel>> GetPreviouslyHandledSubmissionEvents(string ulns)
        {
            var previouslyHandled = await _mediator.SendAsync(new GetPreviouslyHandledSubmissionEventsForEventRequest
            {
                Ulns = ulns
            });

            return new OrchestratorResponse<PreviouslyHandledSubmissionEventViewModel>
            {
                Data =new PreviouslyHandledSubmissionEventViewModel
                {
                    Events = previouslyHandled.PreviouslyHandledSubmissionEvents
                }
            };
        }
    }
}
