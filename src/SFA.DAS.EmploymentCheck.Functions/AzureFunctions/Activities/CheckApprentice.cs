using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
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
        public async Task Verify([ActivityTrigger] Apprentice apprentice)
        {
            var thisMethodName = "CheckApprentice.Verify([ActivityTrigger] ApprenticeToVerifyDto apprentice) activity";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            try
            {
                // Send MediatR request to check the apprentices employment status
                await _mediator.Send(new CheckApprenticeCommand(apprentice));
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}
