﻿using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries.GetHmrcLearnerEmploymentStatus;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetHmrcLearnerEmploymentStatusActivity
    {
        private readonly IMediator _mediator;

        public GetHmrcLearnerEmploymentStatusActivity(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetHmrcLearnerEmploymentStatusActivity))]
        public async Task<EmploymentCheckCacheRequest> GetHmrcEmploymentStatusTask(
            [ActivityTrigger] EmploymentCheckCacheRequest request)
        {
            var checkEmploymentStatusQueryResult = await _mediator.Send(new GetHmrcLearnerEmploymentStatusQueryRequest(request));

            return checkEmploymentStatusQueryResult.EmploymentCheckCacheRequest;
        }
    }
}