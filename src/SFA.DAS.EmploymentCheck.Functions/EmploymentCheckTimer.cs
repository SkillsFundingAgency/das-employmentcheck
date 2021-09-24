using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Commands.InitiateEmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Functions
{
    public class EmploymentCheckTimer
    {
        private readonly IMediator _mediator;

        public EmploymentCheckTimer(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(EmploymentCheckTimer))]
        public async Task Run([TimerTrigger("0 0 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            await _mediator.Send(new InitiateEmploymentCheckRequest());
            
            log.LogInformation($"Employment check triggered at: {DateTime.Now}");
        }
    }
}
