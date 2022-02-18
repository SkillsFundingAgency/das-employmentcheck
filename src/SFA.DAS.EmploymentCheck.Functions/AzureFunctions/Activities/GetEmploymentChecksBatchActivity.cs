using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.GetEmploymentChecksBatch;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetEmploymentChecksBatchActivity
    {
        private readonly IQueryDispatcher _mediator;

        public GetEmploymentChecksBatchActivity(
            IQueryDispatcher mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetEmploymentChecksBatchActivity))]
        public async Task<IList<EmploymentCheck.Data.Models.EmploymentCheck>> Get([ActivityTrigger] object input)
        {
            var result = await _mediator.Send<GetEmploymentCheckBatchQueryRequest, GetEmploymentCheckBatchQueryResult>(new GetEmploymentCheckBatchQueryRequest());

            return result.ApprenticeEmploymentChecks;
        }
    }
}