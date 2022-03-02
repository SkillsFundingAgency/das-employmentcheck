using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries.GetDbNiNumber;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetDbLearnerNiNumberActivity
    {
        private readonly IMediator _mediator;

        public GetDbLearnerNiNumberActivity(
            IMediator mediator)
        {
           _mediator = mediator;
        }

        [FunctionName(nameof(GetDbLearnerNiNumberActivity))]
        public async Task<LearnerNiNumber> Get(
            [ActivityTrigger] Data.Models.EmploymentCheck employmentCheck)
        {
            Guard.Against.Null(employmentCheck, nameof(employmentCheck));

            var getDbLearnerNiNumbersQueryResult = await _mediator.Send(new GetDbNiNumberQueryRequest(employmentCheck));

            return getDbLearnerNiNumbersQueryResult?.LearnerNiNumber ?? new LearnerNiNumber();
        }
    }
}

