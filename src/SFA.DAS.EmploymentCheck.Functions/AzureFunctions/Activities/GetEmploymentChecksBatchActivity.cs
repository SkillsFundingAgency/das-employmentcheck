using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecksBatch;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetEmploymentChecksBatchActivity
    {
        private readonly IMediator _mediator;

        public GetEmploymentChecksBatchActivity(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetEmploymentChecksBatchActivity))]
        public async Task<IList<Models.EmploymentCheck>> Get(
            [ActivityTrigger] object input)
        {
            var result = await _mediator.Send(new GetEmploymentCheckBatchQueryRequest());

            return result.ApprenticeEmploymentChecks ?? new List<Models.EmploymentCheck>();
        }
    }
}