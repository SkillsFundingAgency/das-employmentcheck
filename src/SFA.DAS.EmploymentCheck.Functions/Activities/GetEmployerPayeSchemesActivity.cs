using System.Collections.Generic;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetPayeSchemes;

namespace SFA.DAS.EmploymentCheck.Functions.Activities
{
    public class GetEmployerPayeSchemesActivity
    {
        private readonly IMediator _mediator;

        public GetEmployerPayeSchemesActivity(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(Activities.GetEmployerPayeSchemesActivity))]
        public async Task<IList<EmployerPayeSchemes>> Get([ActivityTrigger] IList<Domain.Entities.EmploymentCheck> employmentCheckBatch)
        {
            Guard.Against.NullOrEmpty(employmentCheckBatch, nameof(employmentCheckBatch));
            var result = await _mediator.Send(new GetPayeSchemesQueryRequest(employmentCheckBatch));

            return result.EmployersPayeSchemes;
        }
    }
}
