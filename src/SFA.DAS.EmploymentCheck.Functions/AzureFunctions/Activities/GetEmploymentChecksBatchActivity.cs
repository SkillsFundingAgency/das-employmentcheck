using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Queries.GetEmploymentChecksBatch;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetEmploymentChecksBatchActivity
    {
        #region Private members
        private readonly IMediator _mediator;
        #endregion Private members

        public GetEmploymentChecksBatchActivity(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetEmploymentChecksBatchActivity))]
        public async Task<IList<EmploymentCheck.Data.Models.EmploymentCheck>> Get(
            [ActivityTrigger] object input)
        {
            var result = await _mediator.Send(new GetEmploymentCheckBatchQueryRequest());

            return result.ApprenticeEmploymentChecks;
        }
    }
}