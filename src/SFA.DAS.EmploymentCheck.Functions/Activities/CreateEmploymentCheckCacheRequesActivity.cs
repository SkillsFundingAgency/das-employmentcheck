using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Domain.Common.Dtos;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests;

namespace SFA.DAS.EmploymentCheck.Functions.Activities
{
    public class CreateEmploymentCheckCacheRequesActivity
    {
        private readonly IMediator _mediator;

        public CreateEmploymentCheckCacheRequesActivity(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(CreateEmploymentCheckCacheRequesActivity))]
        public async Task<Unit> Create([ActivityTrigger] EmploymentCheckData employmentCheckData)
        {
            return await _mediator.Send(new CreateEmploymentCheckCacheRequestCommand(employmentCheckData));
        }
    }
}