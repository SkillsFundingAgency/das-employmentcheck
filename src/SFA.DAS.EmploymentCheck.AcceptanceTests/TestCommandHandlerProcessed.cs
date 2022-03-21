using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.AcceptanceTests.Hooks;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests
{
    public class TestCommandHandlerProcessed<T> : ICommandHandler<T> where T : ICommand
    {
        private readonly ICommandHandler<T> _handler;
        private readonly IHook<ICommand> _hook;

        public TestCommandHandlerProcessed(
            ICommandHandler<T> handler,
            IHook<ICommand> hook)
        {
            _handler = handler;
            _hook = hook;
        }

        public async Task Handle(T command, CancellationToken cancellationToken = default)
        {
            if (_hook != null)
            {
                await _handler.Handle(command, cancellationToken);

                if (_hook?.OnProcessed != null)
                {
                    _hook.OnProcessed(command);
                }
            }
            else
            {
                await _handler.Handle(command, cancellationToken);
            }
        }
    }
}
