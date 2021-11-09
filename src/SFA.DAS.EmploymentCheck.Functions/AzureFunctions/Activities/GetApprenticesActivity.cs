using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprentices;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetApprenticesActivity
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetApprenticesActivity> _logger;

        public GetApprenticesActivity(
            IMediator mediator,
            ILogger<GetApprenticesActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(GetApprenticesActivity))]
        public async Task<IList<Apprentice>> Get([ActivityTrigger] object input)
        {
            var thisMethodName = "GetApprenticesActivity.Get()";

            GetApprenticesMediatorResult getApprenticesResult;
            try
            {
                getApprenticesResult = await _mediator.Send(new GetApprenticesMediatorRequest());
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
                getApprenticesResult = new GetApprenticesMediatorResult(new List<Apprentice>()); //return an empty list instead of null
            }

            return getApprenticesResult.Apprentices;
        }
    }
}

