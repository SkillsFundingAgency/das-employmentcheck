using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Commands.Types;
using SFA.DAS.EmploymentCheck.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.MessageHandlers
{
    public class HandlePublishEmploymentCheckResultCommand
    {
        private readonly ILogger<HandlePublishEmploymentCheckResultCommand> _logger;

        // TODO: delete when Employer Incentives have released a subscriber
        public HandlePublishEmploymentCheckResultCommand(ILogger<HandlePublishEmploymentCheckResultCommand> logger)
        {
            _logger = logger;
        }

        [FunctionName("HandlePublishEmploymentCheckResultCommand")]
        public Task HandleCommand([NServiceBusTrigger(Endpoint = QueueNames.PublishEmploymentCheckResult)] PublishEmploymentCheckResultCommand command)
        {
            _logger.LogInformation(
                "HandlePublishEmploymentCheckResultCommand: CorrelationId={0}, Employed={1}, ErrorType={2}, MessageSentTime=={3}",
                command.CorrelationId,
                command.EmploymentResult,
                command.ErrorType,
                command.CheckDate
            );
            return Task.CompletedTask;
        }
    }
}
