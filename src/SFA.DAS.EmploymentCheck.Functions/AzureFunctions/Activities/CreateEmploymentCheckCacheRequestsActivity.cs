using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class CreateEmploymentCheckCacheRequestsActivity
    {
        private readonly IMediator _mediator;

        public CreateEmploymentCheckCacheRequestsActivity(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(CreateEmploymentCheckCacheRequestsActivity))]
        public async Task<EmploymentCheckCacheRequest> Create([ActivityTrigger] EmploymentCheckData employmentCheckData)
        {
            return (await _mediator.Send(new CreateEmploymentCheckCacheRequestCommand(employmentCheckData))).EmploymentCheckCacheRequest;
        }
    }
}