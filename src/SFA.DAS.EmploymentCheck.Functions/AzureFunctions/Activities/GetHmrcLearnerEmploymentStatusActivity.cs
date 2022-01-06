using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetHmrcLearnerEmploymentStatus;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetHmrcLearnerEmploymentStatusActivity
    {
        #region Private members
        private const string ThisClassName = "\n\nGetHmrcLearnerEmploymentStatusActivity";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";
        private readonly ILogger<GetHmrcLearnerEmploymentStatusActivity> _logger;
        private readonly IMediator _mediator;
        #endregion Private members

        #region Constructors
        public GetHmrcLearnerEmploymentStatusActivity(
            ILogger<GetHmrcLearnerEmploymentStatusActivity> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
        #endregion Constructors

        #region GetHmrcEmploymentStatusTask
        [FunctionName(nameof(GetHmrcLearnerEmploymentStatusActivity))]
        public async Task<EmploymentCheckCacheRequest> GetHmrcEmploymentStatusTask(
            [ActivityTrigger] EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            var thisMethodName = $"{ThisClassName}.GetHmrcEmploymentStatusTask()";

            EmploymentCheckCacheRequest updatedEmploymentCheckCacheRequest = null;
            try
            {
                if (employmentCheckCacheRequest != null)
                {
                    // Send MediatR request to check the employment status using the HMRC API
                    var checkEmploymentStatusQueryResult = await _mediator.Send(new GetHmrcLearnerEmploymentStatusQueryRequest(employmentCheckCacheRequest));

                    if (checkEmploymentStatusQueryResult != null &&
                        checkEmploymentStatusQueryResult.EmploymentCheckCacheRequest != null)
                    {
                        updatedEmploymentCheckCacheRequest = checkEmploymentStatusQueryResult.EmploymentCheckCacheRequest;
                    }
                    else
                    {
                        _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The checkEmploymentStatusQueryResult value returned from the call to CheckEmploymentStatusQueryRequest() is null.");
                        updatedEmploymentCheckCacheRequest = new EmploymentCheckCacheRequest(); // create a blank message for the Mediator result wrapper
                    }
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The input parameter employmentCheckMessage is null.");
                    updatedEmploymentCheckCacheRequest = new EmploymentCheckCacheRequest(); // create a blank message for the Mediator result wrapper
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return updatedEmploymentCheckCacheRequest;
        }
        #endregion GetHmrcEmploymentStatusTask
    }
}