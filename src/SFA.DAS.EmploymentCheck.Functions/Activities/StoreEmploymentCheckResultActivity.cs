using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using SFA.DAS.EmploymentCheck.Application.Mediators.Commands.StoreEmploymentCheckResult;

namespace SFA.DAS.EmploymentCheck.Functions.Activities
{
    public class StoreEmploymentCheckResultActivity
    {
        private readonly IMediator _mediator;

        public StoreEmploymentCheckResultActivity(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(StoreEmploymentCheckResultActivity))]
        public async Task<Unit> StoreEmploymentCheckResultTask([ActivityTrigger] EmploymentCheckCacheRequest employmentCheckCachRequest)
        {
            Guard.Against.Null(employmentCheckCachRequest, nameof(employmentCheckCachRequest));

            await _mediator.Send(new StoreEmploymentCheckResultCommand(employmentCheckCachRequest));

            return await Task.FromResult(Unit.Value);
        }
    }
}