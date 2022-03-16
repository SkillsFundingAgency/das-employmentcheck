using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult
{
    public class PublishEmploymentCheckResultCommandHandler : ICommandHandler<PublishEmploymentCheckResultCommand>
    {
        private readonly ICommandPublisher _commandPublisher;

        public PublishEmploymentCheckResultCommandHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task Handle(PublishEmploymentCheckResultCommand command, CancellationToken cancellationToken = default)
        {
            return _commandPublisher.Publish(command, cancellationToken);
        }
    }
}