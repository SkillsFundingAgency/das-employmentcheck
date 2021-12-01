using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckEmploymentStatus;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class CheckEmploymentStatusActivity
    {
        private const string ThisClassName = "\n\nCheckEmploymentStatusActivity";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        private readonly IMediator _mediator;
        private readonly ILogger<CheckEmploymentStatusActivity> _logger;

        public CheckEmploymentStatusActivity(
            IMediator mediator,
            ILogger<CheckEmploymentStatusActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(CheckEmploymentStatusActivity))]
        public async Task<EmploymentCheckMessage> CheckEmploymentStatusActivityTask(
            [ActivityTrigger] EmploymentCheckMessage employmentCheckMessage)
        {
            var thisMethodName = $"{ThisClassName}.CheckEmploymentStatusActivityTask()";

            EmploymentCheckMessage updatedEmploymentCheckMessage = null;
            try
            {
                if (employmentCheckMessage != null)
                {
                    // Send MediatR request to check the employment status using the HMRC API
                    var checkEmploymentStatusQueryResult = await _mediator.Send(new CheckEmploymentStatusQueryRequest(employmentCheckMessage));

                    if (checkEmploymentStatusQueryResult != null &&
                        checkEmploymentStatusQueryResult.EmploymentCheckMessage != null)
                    {
                        updatedEmploymentCheckMessage = checkEmploymentStatusQueryResult.EmploymentCheckMessage;
                    }
                    else
                    {
                        _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The checkEmploymentStatusQueryResult value returned from the call to CheckEmploymentStatusQueryRequest() is null.");
                        updatedEmploymentCheckMessage = new EmploymentCheckMessage(); // create a blank message for the Mediator result wrapper
                    }
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The input parameter employmentCheckMessage is null.");
                    updatedEmploymentCheckMessage = new EmploymentCheckMessage(); // create a blank message for the Mediator result wrapper
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return updatedEmploymentCheckMessage;
        }
    }
}
