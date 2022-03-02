using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetDbNiNumber;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

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
            [ActivityTrigger] Models.EmploymentCheck employmentCheck)
        {
            Guard.Against.Null(employmentCheck, nameof(employmentCheck));

            var getDbLearnerNiNumbersQueryResult = await _mediator.Send(new GetDbNiNumberQueryRequest(employmentCheck));

            return getDbLearnerNiNumbersQueryResult?.LearnerNiNumber ?? new LearnerNiNumber();
        }
    }
}

