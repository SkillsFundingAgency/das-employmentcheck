using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Types;
using SFA.DAS.NServiceBus.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult
{
    public class PublishEmploymentCheckResultCommandHandler : ICommandHandler<PublishEmploymentCheckResultCommand>
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<PublishEmploymentCheckResultCommandHandler> _logger;

        public PublishEmploymentCheckResultCommandHandler(
            IEventPublisher eventPublisher,
            ILogger<PublishEmploymentCheckResultCommandHandler> logger)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public Task Handle(PublishEmploymentCheckResultCommand command, CancellationToken cancellationToken = default)
        {
            var @event = new EmploymentCheckCompletedEvent(
                command.EmploymentCheck.CorrelationId,
                command.EmploymentCheck.Employed,
                command.EmploymentCheck.LastUpdatedOn ?? command.EmploymentCheck.CreatedOn,
                command.EmploymentCheck.ErrorType
            );

            _logger.LogInformation(
                "PublishEmploymentCheckResultCommandHandler: CorrelationId={0}, Employed={1}, ErrorType={2}, MessageSentTime=={3}",
                @event.CorrelationId,
                @event.EmploymentResult,
                @event.ErrorType,
                @event.CheckDate
            );

            return _eventPublisher.Publish(@event);
        }
    }
}