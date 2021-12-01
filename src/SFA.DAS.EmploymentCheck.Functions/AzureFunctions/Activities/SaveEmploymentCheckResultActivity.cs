using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SaveEmploymentCheckResult;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class SaveEmploymentCheckResultActivity
    {
        private const string ThisClassName = "\n\nSaveEmploymentCheckResultActivity";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        private readonly IMediator _mediator;
        private readonly ILogger<SaveEmploymentCheckResultActivity> _logger;

        public SaveEmploymentCheckResultActivity(
            IMediator mediator,
            ILogger<SaveEmploymentCheckResultActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(SaveEmploymentCheckResultActivity))]
        public async Task SaveApprenticeEmploymentCheckResultActivityTask(
            [ActivityTrigger] EmploymentCheckMessage employmentCheckMessage)
        {
            var thisMethodName = $"{ThisClassName}.SaveEmploymentCheckResultActivityTask";

            try
            {
                if(employmentCheckMessage != null)
                {
                    // Send MediatR request to save the employment status
                    await _mediator.Send(new SaveEmploymentCheckResultCommand(employmentCheckMessage));
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName} The employmentCheckMessage input parameter is null");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}
