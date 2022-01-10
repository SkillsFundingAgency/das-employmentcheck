using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetNiNumbers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Activities
{
    public class GetLearnerNiNumbersActivity
    {
        private readonly IMediator _mediator;

        public GetLearnerNiNumbersActivity(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetLearnerNiNumbersActivity))]
        public async Task<IList<LearnerNiNumber>> Get([ActivityTrigger] IList<Domain.Entities.EmploymentCheck> employmentCheckBatch)
        {
            Guard.Against.NullOrEmpty(employmentCheckBatch, nameof(employmentCheckBatch));

            return (await _mediator.Send(new GetNiNumbersQueryRequest(employmentCheckBatch))).LearnerNiNumber;
        }
    }
}