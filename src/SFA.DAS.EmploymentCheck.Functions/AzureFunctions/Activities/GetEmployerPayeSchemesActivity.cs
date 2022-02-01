using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetPayeSchemes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetEmployerPayeSchemesActivity
    {
        private readonly IMediator _mediator;
        public GetEmployerPayeSchemesActivity(
            IMediator mediator,
            ILogger<GetEmployerPayeSchemesActivity> logger)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetEmployerPayeSchemesActivity))]
        public async Task<IList<EmployerPayeSchemes>> Get(
            [ActivityTrigger] IList<Application.Models.EmploymentCheck> employmentCheckBatch)
        {
            Guard.Against.NullOrEmpty(employmentCheckBatch, nameof(employmentCheckBatch));
            var result = await _mediator.Send(new GetPayeSchemesQueryRequest(employmentCheckBatch));

            return result.EmployersPayeSchemes;
        }
    }
}
