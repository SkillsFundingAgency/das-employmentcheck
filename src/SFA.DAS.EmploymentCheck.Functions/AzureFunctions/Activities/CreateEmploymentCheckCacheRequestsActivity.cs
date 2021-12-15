using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class CreateEmploymentCheckCacheRequestsActivity
    {
        private const string ThisClassName = "\n\nCreateEmploymentCheckCacheRequestsActivity";
        private readonly IMediator _mediator;
        private readonly ILogger<CreateEmploymentCheckCacheRequestsActivity> _logger;

        public CreateEmploymentCheckCacheRequestsActivity(
            IMediator mediator,
            ILogger<CreateEmploymentCheckCacheRequestsActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(CreateEmploymentCheckCacheRequestsActivity))]
        public async Task<IList<EmploymentCheckCacheRequest>> Get(
            [ActivityTrigger] IList<EmploymentCheckModel> employmentCheckModels)
        {
            var thisMethodName = $"{ThisClassName}.Get()";

            CreateEmploymentCheckCacheRequestsCommandResult createEmploymentCheckCacheRequestsQueryResult;
            try
            {
                createEmploymentCheckCacheRequestsQueryResult = await _mediator.Send(new CreateEmploymentCheckCacheRequestsCommandRequest(employmentCheckModels));
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
                createEmploymentCheckCacheRequestsQueryResult = new CreateEmploymentCheckCacheRequestsCommandResult(new List<EmploymentCheckCacheRequest>()); //returns empty list instead of null
            }

            return createEmploymentCheckCacheRequestsQueryResult.EmploymentCheckCacheRequests;
        }
    }
}

