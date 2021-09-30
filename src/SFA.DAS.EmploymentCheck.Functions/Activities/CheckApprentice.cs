using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Commands.CheckApprentice;
using SFA.DAS.EmploymentCheck.Functions.Dtos;

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
            _logger.LogInformation($"Checking apprentice {apprentice.ULN}.", apprentice.ULN);
            await _mediator.Send(new CheckApprenticeCommand(apprentice));
            _logger.LogInformation($"Completed check for apprentice {apprentice.ULN}.", apprentice.ULN);
        }
    }
}
