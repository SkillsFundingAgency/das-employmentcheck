using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckApprenticeEmploymentStatus;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckEmploymentStatus;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class CheckApprenticeEmploymentStatusActivity
    {
        private const string ThisClassName = "\n\nCheckApprenticeEmploymentStatusActivity";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        private readonly IMediator _mediator;
        private readonly ILogger<SaveEmploymentCheckResultActivity> _logger;

        public CheckApprenticeEmploymentStatusActivity(
            IMediator mediator,
            ILogger<SaveEmploymentCheckResultActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(CheckApprenticeEmploymentStatusActivity))]
        public async Task<EmploymentCheckMessage> CheckApprenticeEmploymentStatusActivityTask(
            [ActivityTrigger] EmploymentCheckMessage employmentCheckMessage)
        {
            var thisMethodName = $"{ThisClassName}.CheckApprenticeEmploymentStatusActivityTask()";

            EmploymentCheckMessage updatedEmploymentCheckMessage = null;
            try
            {
                if (employmentCheckMessage != null)
                {
                    // Send MediatR request to check the apprentices employment status using the HMRC API
                    var checkApprenticeEmploymentStatusQueryResult = await _mediator.Send(new CheckEmploymentStatusQueryRequest(employmentCheckMessage));

                    if (checkApprenticeEmploymentStatusQueryResult != null &&
                        checkApprenticeEmploymentStatusQueryResult.EmploymentCheckMessage != null)
                    {
                        updatedEmploymentCheckMessage = checkApprenticeEmploymentStatusQueryResult.EmploymentCheckMessage;
                    }
                    else
                    {
                        _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The checkEmploymentStatusQueryResult value returned from the call to CheckEmploymentStatusQueryRequest() is null.");
                    }
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The input parameter employmentCheckMessage is null.");
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
