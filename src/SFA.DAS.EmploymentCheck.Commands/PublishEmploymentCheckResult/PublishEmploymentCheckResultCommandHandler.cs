using NServiceBus;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult
{
    public class PublishEmploymentCheckResultCommandHandler : ICommandHandler<PublishEmploymentCheckResultCommand>
    {
        private readonly IMessageSession _eventPublisher;

        public PublishEmploymentCheckResultCommandHandler(Lazy<IMessageSession> eventPublisher)
        {
            _eventPublisher = eventPublisher.Value;
        }

        public Task Handle(PublishEmploymentCheckResultCommand command, CancellationToken cancellationToken = default)
        {
            var @event = new EmploymentCheckCompletedEvent(
                command.EmploymentCheck.CorrelationId,
                command.EmploymentCheck.Employed,
                command.EmploymentCheck.LastUpdatedOn ?? command.EmploymentCheck.CreatedOn,
                command.EmploymentCheck.ErrorType
            );

            return _eventPublisher.Send(@event);
        }
    }
}