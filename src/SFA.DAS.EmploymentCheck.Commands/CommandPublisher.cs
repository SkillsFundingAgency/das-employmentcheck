using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.NServiceBus.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands
{
    public class CommandPublisher : ICommandPublisher
    {
        private readonly IEventPublisher _eventPublisher;

        public CommandPublisher(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public Task Publish<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand
        {
            return _eventPublisher.Publish(command);
        }
    }
}
