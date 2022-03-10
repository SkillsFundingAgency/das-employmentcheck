using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Commands.StoreCompletedEmploymentCheck;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class StoreCompletedEmploymentCheckActivity
    {
        private readonly IMediator _mediator;

        public StoreCompletedEmploymentCheckActivity(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(StoreCompletedEmploymentCheckActivity))]
        public async Task Store([ActivityTrigger] Models.EmploymentCheck employmentCheck)
        {
            await _mediator.Send(new StoreCompletedEmploymentCheckCommand(employmentCheck));
        }
    }
}