using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SaveApprenticeEmploymentCheckResult;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class SaveApprenticeEmploymentCheckResultActivity
    {
        private const string ThisClassName = "\n\nSaveApprenticeEmploymentCheckResultsActivity";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        private readonly IMediator _mediator;
        private readonly ILogger<SaveApprenticeEmploymentCheckResultActivity> _logger;

        public SaveApprenticeEmploymentCheckResultActivity(
            IMediator mediator,
            ILogger<SaveApprenticeEmploymentCheckResultActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(SaveApprenticeEmploymentCheckResultActivity))]
        public async Task SaveApprenticeEmploymentCheckResultActivityTask(
            [ActivityTrigger] ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel)
        {
            var thisMethodName = $"{ThisClassName}.SaveApprenticeEmploymentCheckResultsActivityTask";

            try
            {
                if(apprenticeEmploymentCheckMessageModel != null)
                {
                    // Send MediatR request to save the apprentices employment status
                    await _mediator.Send(new SaveApprenticeEmploymentCheckResultCommand(apprenticeEmploymentCheckMessageModel));
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName} The apprenticeEmploymentCheckMessageModel input parameter is null");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}
