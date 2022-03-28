using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Types;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.MessageHandlers
{
    public class HandleEmploymentCheckCompletedEvent
    {
        private readonly ILogger<HandleEmploymentCheckCompletedEvent> _logger;

        public HandleEmploymentCheckCompletedEvent(ILogger<HandleEmploymentCheckCompletedEvent> logger)
        {
            _logger = logger;
        }

        [FunctionName("HandleEmploymentCheckCompletedEvent")]
        public Task Handle(
            [NServiceBusTrigger(Endpoint = QueueNames.PublishEmploymentCheckResult)]
            EmploymentCheckCompletedEvent @event)
        {
            _logger.LogInformation(
                "HandleEmploymentCheckCompletedEvent: CorrelationId={0}, Employed={1}, ErrorType={2}, MessageSentTime=={3}",
                @event.CorrelationId,
                @event.EmploymentResult,
                @event.ErrorType,
                @event.CheckDate
            );
            return Task.CompletedTask;
        }
    }
}
