using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Commands.Types;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult
{
    public class EmploymentCheckCompletedEventHandler : ICommandHandler<EmploymentCheckCompletedEvent>
    {
        private readonly ICommandPublisher _commandPublisher;

        public EmploymentCheckCompletedEventHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task Handle(EmploymentCheckCompletedEvent @event, CancellationToken cancellationToken = default)
        {
            var command = new PublishEmploymentCheckResultCommand(
                @event.EmploymentCheck.CorrelationId,
                @event.EmploymentCheck.Employed,
                @event.EmploymentCheck.LastUpdatedOn ?? @event.EmploymentCheck.CreatedOn,
                @event.EmploymentCheck.ErrorType
            );

            return _commandPublisher.Publish(command, cancellationToken);
        }
    }
}