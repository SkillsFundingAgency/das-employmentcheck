using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetNiNumber;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetLearnerNiNumberActivity
    {
        private readonly IMediator _mediator;

        public GetLearnerNiNumberActivity(
            IMediator mediator,
            ILogger<GetLearnerNiNumberActivity> logger)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetLearnerNiNumberActivity))]
        public async Task<LearnerNiNumber> Get(
            [ActivityTrigger] Application.Models.EmploymentCheck employmentCheck)
        {
            if (employmentCheckBatch == null || employmentCheckBatch.Count == 0)
            {
                return new List<LearnerNiNumber>();
            }

            var getLearnerNiNumbersQueryResult = await _mediator.Send(new GetNiNumbersQueryRequest(employmentCheckBatch));

            return getLearnerNiNumbersQueryResult.LearnerNiNumber;
        }
    }
}

