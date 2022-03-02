using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries.GetPayeSchemes;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetEmployerPayeSchemesActivity
    {
        private readonly IMediator _mediator;
        public GetEmployerPayeSchemesActivity(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetEmployerPayeSchemesActivity))]
        public async Task<EmployerPayeSchemes> Get(
            [ActivityTrigger] Data.Models.EmploymentCheck employmentCheck)
        {
            Guard.Against.Null(employmentCheck, nameof(employmentCheck));
            var result = await _mediator.Send(new GetPayeSchemesQueryRequest(employmentCheck));

            return result.EmployersPayeSchemes ?? new EmployerPayeSchemes();
        }
    }
}
