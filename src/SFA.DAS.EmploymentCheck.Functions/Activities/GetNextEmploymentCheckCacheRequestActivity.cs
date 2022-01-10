using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetNextEmploymentCheckCacheRequest;
using SFA.DAS.EmploymentCheck.Domain.Entities;

namespace SFA.DAS.EmploymentCheck.Functions.GetEmploymentCheckCacheRequest
{
    public class GetNextEmploymentCheckCacheRequestActivity
    {
        private readonly IMediator _mediator;

        public GetNextEmploymentCheckCacheRequestActivity(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetNextEmploymentCheckCacheRequestActivity))]
        public async Task<EmploymentCheckCacheRequest> Get([ActivityTrigger] object input)
        {
            _ = input;

            return (await _mediator.Send(new GetNextEmploymentCheckCacheRequestQueryRequest())).EmploymentCheckCacheRequest;
        }
    }
}