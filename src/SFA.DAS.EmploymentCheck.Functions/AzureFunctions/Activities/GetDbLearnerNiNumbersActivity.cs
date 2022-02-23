using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetDbNiNumbers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetDbLearnerNiNumbersActivity
    {
        private readonly ILogger<GetDbLearnerNiNumbersActivity> _logger;
        private readonly IMediator _mediator;

        public GetDbLearnerNiNumbersActivity(
            ILogger<GetDbLearnerNiNumbersActivity> logger,
            IMediator mediator)
        {
            _logger = logger;
           _mediator = mediator;
        }

        [FunctionName(nameof(GetDbLearnerNiNumbersActivity))]
        public async Task<IList<LearnerNiNumber>> Get(
            [ActivityTrigger] IList<Models.EmploymentCheck> employmentCheckBatch)
        {
            Guard.Against.NullOrEmpty(employmentCheckBatch, nameof(employmentCheckBatch));

            var getDbLearnerNiNumbersQueryResult = await _mediator.Send(new GetDbNiNumbersQueryRequest(employmentCheckBatch));
            if (getDbLearnerNiNumbersQueryResult != null &&
                getDbLearnerNiNumbersQueryResult.LearnerNiNumbers != null &&
                getDbLearnerNiNumbersQueryResult.LearnerNiNumbers.Count > 0)
            {
                _logger.LogInformation($"{nameof(GetDbLearnerNiNumbersActivity)} returned {getDbLearnerNiNumbersQueryResult?.LearnerNiNumbers.Count} NiNumbers");

                foreach (var learnerNiNumber in getDbLearnerNiNumbersQueryResult?.LearnerNiNumbers)
                {
                    _logger.LogInformation($"A nino was found in the database for ULN [{learnerNiNumber.Uln}]");
                }
            }

            return getDbLearnerNiNumbersQueryResult?.LearnerNiNumbers ?? new List<LearnerNiNumber>();
        }
    }
}

