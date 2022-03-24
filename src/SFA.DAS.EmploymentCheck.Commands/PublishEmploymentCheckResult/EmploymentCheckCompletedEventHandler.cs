using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Abstractions;

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
            var command = new PublishEmploymentCheckResultCommand(@event.EmploymentCheck);

            return _commandPublisher.Publish(command, cancellationToken);
        }
    }
}