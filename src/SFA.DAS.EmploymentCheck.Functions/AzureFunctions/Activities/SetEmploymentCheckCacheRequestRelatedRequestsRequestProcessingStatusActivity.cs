using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusActivity
    {
        private readonly IMediator _mediator;

        public SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusActivity(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusActivity))]
        public async Task<IList<EmploymentCheckCacheRequest>> SetEmploymentCheckCacheRequestsRelatedRequestsRequestProcessingStatus(
            [ActivityTrigger] Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> employmentCheckCacheRequestAndStatusToSet
        )
        {
            return (await _mediator.Send(new SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusCommand(employmentCheckCacheRequestAndStatusToSet))).EmploymentCheckCacheRequests;
        }
    }
}