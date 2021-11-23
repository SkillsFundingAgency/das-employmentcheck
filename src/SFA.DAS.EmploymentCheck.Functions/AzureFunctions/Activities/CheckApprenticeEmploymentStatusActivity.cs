using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckApprenticeEmploymentStatus;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class CheckApprenticeEmploymentStatusActivity
    {
        private const string ThisClassName = "\n\nCheckApprenticeEmploymentStatusActivity";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        private readonly IMediator _mediator;
        private readonly ILogger<SaveApprenticeEmploymentCheckResultActivity> _logger;

        public CheckApprenticeEmploymentStatusActivity(
            IMediator mediator,
            ILogger<SaveApprenticeEmploymentCheckResultActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(CheckApprenticeEmploymentStatusActivity))]
        public async Task<ApprenticeEmploymentCheckMessageModel> CheckApprenticeEmploymentStatusActivityTask(
            [ActivityTrigger] ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel)
        {
            var thisMethodName = $"{ThisClassName}.CheckApprenticeEmploymentStatusActivityTask()";

            ApprenticeEmploymentCheckMessageModel updatedApprenticeEmploymentCheckMessageModel = null;
            try
            {
                if (apprenticeEmploymentCheckMessageModel != null)
                {
                    // Send MediatR request to check the apprentices employment status using the HMRC API
                    var checkApprenticeEmploymentStatusQueryResult = await _mediator.Send(new CheckApprenticeEmploymentStatusQueryRequest(apprenticeEmploymentCheckMessageModel));

                    if (checkApprenticeEmploymentStatusQueryResult != null &&
                        checkApprenticeEmploymentStatusQueryResult.ApprenticeEmploymentCheckMessageModel != null)
                    {
                        updatedApprenticeEmploymentCheckMessageModel = checkApprenticeEmploymentStatusQueryResult.ApprenticeEmploymentCheckMessageModel;
                    }
                    else
                    {
                        _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The checkApprenticeEmploymentStatusQueryResult value returned from the call to CheckApprenticeEmploymentStatusQueryRequest() is null.");
                        updatedApprenticeEmploymentCheckMessageModel = new ApprenticeEmploymentCheckMessageModel(); // create a blank message for the Mediator result wrapper
                    }
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The input parameter apprenticeEmploymentCheckMessageModel is null.");
                    updatedApprenticeEmploymentCheckMessageModel = new ApprenticeEmploymentCheckMessageModel(); // create a blank message for the Mediator result wrapper
                }

            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return updatedApprenticeEmploymentCheckMessageModel;
        }
    }
}
