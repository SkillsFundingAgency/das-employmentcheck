using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetLearnerEmploymentStatus;

namespace SFA.DAS.EmploymentCheck.Functions.Activities
{
    public class GetLearnerEmploymentStatusActivity
    {
        private readonly IMediator _mediator;

        public GetLearnerEmploymentStatusActivity(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetLearnerEmploymentStatusActivity))]
        public async Task<EmploymentCheckCacheRequest> Get([ActivityTrigger] EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            return (await _mediator.Send(new GetLearnerEmploymentStatusQueryRequest(employmentCheckCacheRequest))).EmploymentCheckCacheRequest;
        }
    }
}