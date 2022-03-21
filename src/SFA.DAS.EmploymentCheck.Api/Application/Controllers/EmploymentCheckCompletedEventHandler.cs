using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult;

namespace SFA.DAS.EmploymentCheck.Api.Application.Controllers
{
    public class EmploymentCheckCompletedEventHandler : ICommandHandler<EmploymentCheckCompletedEvent>
    {
        private readonly ICommandPublisher _commandPublisher;

        public EmploymentCheckCompletedEventHandler(ICommandPublisher commandPublisher = null)
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