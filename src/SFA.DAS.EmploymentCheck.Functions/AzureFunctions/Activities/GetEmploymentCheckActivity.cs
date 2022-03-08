using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Queries.GetEmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetEmploymentCheckActivity
    {
        private readonly IMediator _mediator;

        public GetEmploymentCheckActivity(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetEmploymentCheckActivity))]
        public async Task<Data.Models.EmploymentCheck> Get(
            [ActivityTrigger] object input)
        {
            var result = await _mediator.Send(new GetEmploymentCheckQueryRequest());

            return result.EmploymentCheck;
        }
    }
}