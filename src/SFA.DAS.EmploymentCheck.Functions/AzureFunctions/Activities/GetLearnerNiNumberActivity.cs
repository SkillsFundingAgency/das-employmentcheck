using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries.GetNiNumber;

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
            [ActivityTrigger] Data.Models.EmploymentCheck employmentCheck)
        {
            if (employmentCheck == null)
            {
                return new LearnerNiNumber();
            }

            var getLearnerNiNumbersQueryResult = await _mediator.Send(new GetNiNumberQueryRequest(employmentCheck));

            return getLearnerNiNumbersQueryResult.LearnerNiNumber;
        }
    }
}

