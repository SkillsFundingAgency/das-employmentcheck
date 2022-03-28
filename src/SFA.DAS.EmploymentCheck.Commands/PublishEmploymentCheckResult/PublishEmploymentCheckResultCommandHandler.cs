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

        public PublishEmploymentCheckResultCommandHandler(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public Task Handle(PublishEmploymentCheckResultCommand command, CancellationToken cancellationToken = default)
        {
            var @event = new EmploymentCheckCompletedEvent(
                command.EmploymentCheck.CorrelationId,
                command.EmploymentCheck.Employed,
                command.EmploymentCheck.LastUpdatedOn ?? command.EmploymentCheck.CreatedOn,
                command.EmploymentCheck.ErrorType
            );

            return _eventPublisher.Publish(@event);
        }
    }
}