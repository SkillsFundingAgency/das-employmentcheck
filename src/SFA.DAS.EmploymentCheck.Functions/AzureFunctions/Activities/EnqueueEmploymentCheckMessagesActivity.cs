using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.EnqueueEmploymentCheckMessages;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class EnqueueEmploymentCheckMessagesActivity
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EnqueueEmploymentCheckMessagesActivity> _logger;

        public EnqueueEmploymentCheckMessagesActivity(
            IMediator mediator,
            ILogger<EnqueueEmploymentCheckMessagesActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(EnqueueEmploymentCheckMessagesActivity))]
        public async Task<int> Enqueue(
            [ActivityTrigger] EmploymentCheckData apprenticeEmploymentData)
        {
            var thisMethodName = "EnqueueEmploymentCheckMessagesActivity.Enqueue()";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            try
            {
                // Send MediatR request to enqueue the apprentices employment check messages
                await _mediator.Send(new EnqueueEmploymentCheckMessagesCommand(apprenticeEmploymentData));
            }
            catch (Exception ex)
            {
                _logger.LogError($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(0);
        }
    }
}
