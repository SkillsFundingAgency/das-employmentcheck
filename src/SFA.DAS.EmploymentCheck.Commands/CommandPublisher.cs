using NServiceBus;
using SFA.DAS.EmploymentCheck.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using ICommand = SFA.DAS.EmploymentCheck.Abstractions.ICommand;

namespace SFA.DAS.EmploymentCheck.Commands
{
    public class CommandPublisher : ICommandPublisher
    {
        private readonly IMessageSession _eventPublisher;

        public CommandPublisher(Lazy<IMessageSession> eventPublisher)
        {
            _eventPublisher = eventPublisher.Value;
        }

        public Task Publish<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand
        {
            return _eventPublisher.Send(command);
        }
    }
}
