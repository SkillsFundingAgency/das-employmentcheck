using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetDbNiNumbers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetDbLearnerNiNumbersActivity
    {
        private readonly IMediator _mediator;

        public GetDbLearnerNiNumbersActivity(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetDbLearnerNiNumbersActivity))]
        public async Task<IList<LearnerNiNumber>> Get(
            [ActivityTrigger] IList<Models.EmploymentCheck> employmentCheckBatch)
        {
            Guard.Against.NullOrEmpty(employmentCheckBatch, nameof(employmentCheckBatch));

            var getDbLearnerNiNumbersQueryResult = await _mediator.Send(new GetDbNiNumbersQueryRequest(employmentCheckBatch));

            return getDbLearnerNiNumbersQueryResult.LearnerNiNumbers ?? new List<LearnerNiNumber>();
        }
    }
}

