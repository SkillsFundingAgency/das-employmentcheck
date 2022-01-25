using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Data.Models;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequests;

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
        public async Task Create([ActivityTrigger] EmploymentCheckData employmentCheckData)
        {
            await _mediator.Send(new CreateEmploymentCheckCacheCommand(employmentCheckData));
        }
    }
}