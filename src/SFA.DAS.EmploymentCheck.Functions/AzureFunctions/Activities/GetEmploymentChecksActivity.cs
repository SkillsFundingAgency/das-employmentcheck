using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetEmploymentChecksActivity
    {
        private const string ThisClassName = "\n\nGetEmploymentChecksActivity";

        private readonly IMediator _mediator;
        private readonly ILogger<GetEmploymentChecksActivity> _logger;

        public GetEmploymentChecksActivity(
            IMediator mediator,
            ILogger<GetEmploymentChecksActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(GetEmploymentChecksActivity))]
        public async Task<IList<Application.Models.Domain.EmploymentCheckModel>> Get(
            [ActivityTrigger] long employmentCheckLastHighestBatchId)
        {
            var thisMethodName = $"{ThisClassName}.Get()";

            GetEmploymentChecksQueryResult getApprenticesResult;
            try
            {
                getApprenticesResult = await _mediator.Send(new GetEmploymentChecksQueryRequest(employmentCheckLastHighestBatchId));
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
                getApprenticesResult = new GetEmploymentChecksQueryResult(new List<Application.Models.Domain.EmploymentCheckModel>()); //return an empty list instead of null
            }

            return getApprenticesResult.EmploymentCheckModels;
        }
    }
}

