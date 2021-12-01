using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.DequeueEmploymentCheckMessage;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class DequeueEmploymentCheckMessageActivity
    {
        private const string ThisClassName = "\n\nDequeueEmploymentCheckMessageActivity";

        private readonly IMediator _mediator;
        private readonly ILogger<DequeueEmploymentCheckMessageActivity> _logger;

        public DequeueEmploymentCheckMessageActivity(
            IMediator mediator,
            ILogger<DequeueEmploymentCheckMessageActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(DequeueEmploymentCheckMessageActivity))]
        public async Task<EmploymentCheckMessage> DequeueEmploymentCheckMessageActivityTask(
            [ActivityTrigger] object input)
        {
            var thisMethodName = $"{ThisClassName}.DequeueEmploymentCheckMessageActivityTask()";

            EmploymentCheckMessage employmentCheckMessage = null;
            try
            {
                // Send MediatR request to get the next message off the queue
                var dequeueEmploymentCheckMessageQueryRequestResult = await _mediator.Send(new DequeueEmploymentCheckMessageQueryRequest());

                if (dequeueEmploymentCheckMessageQueryRequestResult != null &&
                    dequeueEmploymentCheckMessageQueryRequestResult.EmploymentCheckMessage != null)
                {
                    employmentCheckMessage = dequeueEmploymentCheckMessageQueryRequestResult.EmploymentCheckMessage;
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: The dequeueEmploymentCheckMessageQueryRequestResult value returned from the call to DequeueEmploymentCheckMessageQueryRequest() is null.");
                    employmentCheckMessage = new EmploymentCheckMessage(); // create a blank message for the Mediator result wrapper
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return employmentCheckMessage;
        }
    }
}
