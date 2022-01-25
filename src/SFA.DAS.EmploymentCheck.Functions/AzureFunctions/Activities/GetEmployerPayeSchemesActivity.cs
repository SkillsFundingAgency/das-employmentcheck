using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Queries.GetPayeSchemes;

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
            [ActivityTrigger] IList<EmploymentCheck.Data.Models.EmploymentCheck> employmentCheckBatch)
        {
            Guard.Against.NullOrEmpty(employmentCheckBatch, nameof(employmentCheckBatch));
            var result = await _mediator.Send(new GetPayeSchemesQueryRequest(employmentCheckBatch));

            return result.EmployersPayeSchemes;
        }
    }
}
