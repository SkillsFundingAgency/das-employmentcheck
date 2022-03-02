using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequest;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class CreateEmploymentCheckCacheRequestActivity
    {
        private readonly IMediator _mediator;

        public CreateEmploymentCheckCacheRequestActivity(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(CreateEmploymentCheckCacheRequestActivity))]
        public async Task Create([ActivityTrigger] EmploymentCheckData employmentCheckData)
        {
            await _mediator.Send(new CreateEmploymentCheckCacheRequestCommand(employmentCheckData));
        }
    }
}