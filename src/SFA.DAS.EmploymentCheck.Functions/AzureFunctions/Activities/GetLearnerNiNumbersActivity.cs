using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetNiNumbers;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetLearnerNiNumbersActivity
    {
        private readonly IMediator _mediator;

        public GetLearnerNiNumbersActivity(
            IMediator mediator,
            ILogger<GetLearnerNiNumbersActivity> logger)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetLearnerNiNumbersActivity))]
        public async Task<LearnerNiNumber> Get(
            [ActivityTrigger] Application.Models.EmploymentCheck employmentCheckBatch)
        {
            var getLearnerNiNumbersQueryResult = await _mediator.Send(new GetNiNumbersQueryRequest(employmentCheckBatch));

            return getLearnerNiNumbersQueryResult.LearnerNiNumber;
        }
    }
}

