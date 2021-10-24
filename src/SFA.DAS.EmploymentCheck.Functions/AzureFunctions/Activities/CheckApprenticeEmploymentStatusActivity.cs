using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class CheckApprenticeEmploymentStatusActivity
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CheckApprentice> _logger;

        public CheckApprenticeEmploymentStatusActivity(IMediator mediator, ILogger<CheckApprentice> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(CheckApprenticeEmploymentStatusActivity))]
        public async Task Verify([ActivityTrigger] object allLists)
        {
            var thisMethodName = "CheckApprenticeEmploymentStatusActivity.Verify()";

            try
            {
                // TODO: Implementation
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}
