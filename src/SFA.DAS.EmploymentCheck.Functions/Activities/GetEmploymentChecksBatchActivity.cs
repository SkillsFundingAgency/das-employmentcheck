using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetEmploymentChecksBatch;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Activities
{
    public class GetEmploymentChecksBatchActivity
    {
        private readonly IMediator _mediator;

        public GetEmploymentChecksBatchActivity(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetEmploymentChecksBatchActivity))]
        public async Task<IList<Domain.Entities.EmploymentCheck>> Get([ActivityTrigger] object input)
        {
            _ = input;

            return (await _mediator.Send(new GetEmploymentCheckBatchQueryRequest())).EmploymentChecks;
        }
    }
}