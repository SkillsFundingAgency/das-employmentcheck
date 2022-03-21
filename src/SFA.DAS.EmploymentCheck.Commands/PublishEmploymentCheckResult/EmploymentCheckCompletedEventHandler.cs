using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Abstractions;

namespace SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult
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

            try
            {
                return _commandPublisher.Publish(command, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}