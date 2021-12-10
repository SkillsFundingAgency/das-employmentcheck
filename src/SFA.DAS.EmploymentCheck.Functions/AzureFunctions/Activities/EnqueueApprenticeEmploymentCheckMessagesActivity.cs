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
    public class EnqueueApprenticeEmploymentCheckMessagesActivity
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EnqueueApprenticeEmploymentCheckMessagesActivity> _logger;

        public EnqueueApprenticeEmploymentCheckMessagesActivity(
            IMediator mediator,
            ILogger<EnqueueApprenticeEmploymentCheckMessagesActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(EnqueueApprenticeEmploymentCheckMessagesActivity))]
        public async Task<int> Enqueue(
            [ActivityTrigger] ApprenticeRelatedData apprenticeEmploymentData)
        {
            var thisMethodName = "EnqueueApprenticesEmploymentCheckMessagesActivity.Enqueue()";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            try
            {
                // Send MediatR request to enqueue the apprentices employment check messages
                await _mediator.Send(new EnqueueApprenticeEmploymentCheckMessagesCommand(apprenticeEmploymentData));
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(0);
        }
    }
}
