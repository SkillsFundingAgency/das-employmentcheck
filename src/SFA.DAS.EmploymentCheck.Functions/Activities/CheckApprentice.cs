using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Dtos;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice;

namespace SFA.DAS.EmploymentCheck.Functions.Activities
{
    public class CheckApprentice
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CheckApprentice> _logger;

        public CheckApprentice(IMediator mediator, ILogger<CheckApprentice> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(CheckApprentice))]
        public async Task Verify([ActivityTrigger] ApprenticeToVerifyDto apprentice)
        {
            var thisMethodName = "*** CheckApprentice.Verify([ActivityTrigger] ApprenticeToVerifyDto apprentice) activity";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            try
            {
                // Send MediatR request to get the apprentices for the employment check
                await _mediator.Send(new CheckApprenticeCommand(apprentice));
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}
