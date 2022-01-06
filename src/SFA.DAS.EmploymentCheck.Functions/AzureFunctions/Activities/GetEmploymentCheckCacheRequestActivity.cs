using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.ProcessEmploymentCheckCacheRequest;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetEmploymentCheckCacheRequestActivity
    {
        #region Private members
        private const string ThisClassName = "\n\nProcessEmploymentCheckRequestActivity";
        private readonly IMediator _mediator;
        private readonly ILogger<GetEmploymentCheckCacheRequestActivity> _logger;
        #endregion Private members

        #region Constructors
        public GetEmploymentCheckCacheRequestActivity(
            IMediator mediator,
            ILogger<GetEmploymentCheckCacheRequestActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        #endregion Constructors

        #region GetEmploymentCheckRequestActivity
        [FunctionName(nameof(GetEmploymentCheckCacheRequestActivity))]
        public async Task<EmploymentCheckCacheRequest> GetEmploymentCheckRequestActivityTask(
            [ActivityTrigger] object input)
        {
            var thisMethodName = $"{ThisClassName}.GetEmploymentCheckRequestActivityTask()";

            EmploymentCheckCacheRequest employmentCheckCacheRequest = null;
            try
            {
                // Send MediatR request to get the next request
                var processEmploymentCheckCacheRequestQueryResult = await _mediator.Send(new ProcessEmploymentCheckCacheRequestQueryRequest());

                if (processEmploymentCheckCacheRequestQueryResult != null &&
                    processEmploymentCheckCacheRequestQueryResult.EmploymentCheckCacheRequest != null)
                {
                    employmentCheckCacheRequest = processEmploymentCheckCacheRequestQueryResult.EmploymentCheckCacheRequest;
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: The GetEmploymentCheckMessageQueryRequestResult value returned from the call to ProcessEmploymentCheckMessageQueryRequestResult() is null.");
                    employmentCheckCacheRequest = new EmploymentCheckCacheRequest(); // create a blank message for the Mediator result wrapper
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return employmentCheckCacheRequest;
        }
        #endregion GetEmploymentCheckRequestActivity
    }
}