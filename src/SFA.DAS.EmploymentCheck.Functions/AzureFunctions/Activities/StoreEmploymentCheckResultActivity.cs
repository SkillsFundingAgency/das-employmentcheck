using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.StoreEmploymentCheckResult;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class StoreEmploymentCheckResultActivity
    {
        #region Private members
        private const string ThisClassName = "\n\nStoreEmploymentCheckResultActivity";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";
        private readonly ILogger<StoreEmploymentCheckResultActivity> _logger;
        private readonly IMediator _mediator;
        #endregion Private members

        #region Constructors
        public StoreEmploymentCheckResultActivity(
            ILogger<StoreEmploymentCheckResultActivity> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
        #endregion Constructors

        #region StoreEmploymentCheckResultTask
        [FunctionName(nameof(StoreEmploymentCheckResultActivity))]
        public async Task StoreEmploymentCheckResultTask(
            [ActivityTrigger] EmploymentCheckCacheRequest employmentCheckCachRequest)
        {
            var thisMethodName = $"{ThisClassName}.StoreEmploymentCheckResultTask";

            try
            {
                if (employmentCheckCachRequest != null)
                {
                    // Send MediatR request to save the employment status
                    await _mediator.Send(new StoreEmploymentCheckResultCommand(employmentCheckCachRequest));
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
        #endregion StoreEmploymentCheckResultTask
    }
}