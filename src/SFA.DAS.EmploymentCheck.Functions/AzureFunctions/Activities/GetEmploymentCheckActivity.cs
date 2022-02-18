using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentCheck;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

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
        public async Task<IList<Models.EmploymentCheck>> Get(
            [ActivityTrigger] object input)
        {
            var result = await _mediator.Send(new GetEmploymentCheckQueryRequest());

            return result.ApprenticeEmploymentChecks ?? new List<Models.EmploymentCheck>();
        }
    }
}