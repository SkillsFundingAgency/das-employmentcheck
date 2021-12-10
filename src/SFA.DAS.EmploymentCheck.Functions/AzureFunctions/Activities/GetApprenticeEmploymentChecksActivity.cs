using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticeEmploymentChecks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetApprenticeEmploymentChecksActivity
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetApprenticeEmploymentChecksActivity> _logger;

        public GetApprenticeEmploymentChecksActivity(
            IMediator mediator,
            ILogger<GetApprenticeEmploymentChecksActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(GetApprenticeEmploymentChecksActivity))]
        public async Task<IList<ApprenticeEmploymentCheckModel>> Get(
            [ActivityTrigger] long employmentCheckLastGetId)
        {
            GetApprenticeEmploymentChecksQueryResult getApprenticesResult;
            try
            {
                getApprenticesResult = await _mediator.Send(new GetApprenticeEmploymentChecksQueryRequest(employmentCheckLastGetId));
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{nameof(GetApprenticeEmploymentChecksActivity)}: Exception caught - {ex.Message}. {ex.StackTrace}");
                getApprenticesResult = new GetApprenticeEmploymentChecksQueryResult(new List<ApprenticeEmploymentCheckModel>()); //return an empty list instead of null
            }

            return getApprenticesResult.ApprenticeEmploymentChecks;
        }
    }
}

