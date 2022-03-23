using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.AcceptanceTests.Hooks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests
{
    public class TestCommandPublisher : ICommandPublisher
    {
        private readonly ICommandPublisher _commandPublisher;
        private readonly IHook<ICommand> _hook;

        public TestCommandPublisher(ICommandPublisher commandPublisher, IHook<ICommand> hook)
        {
            _commandPublisher = commandPublisher;
            _hook = hook;
        }

        public async Task Publish<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand
        {
            if (_hook != null)
            {
                var suppressError = false;
                try
                {
                    _hook?.OnReceived?.Invoke(command);
                   
                    await _commandPublisher.Publish(command, cancellationToken);

                    _hook?.OnPublished?.Invoke(command);
                }
                catch (Exception ex)
                {
                    if (_hook?.OnErrored != null)
                    {
                        suppressError = _hook.OnErrored(ex, command);
                    }
                    if (!suppressError)
                    {
                        throw;
                    }
                }
            }
            else
            {
                await _commandPublisher.Publish(command, cancellationToken);
            }
        }
    }
}
