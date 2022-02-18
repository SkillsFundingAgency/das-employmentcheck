using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands
{
    public interface ICommandDispatcher
    {
        Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;
    }

    public interface ICommand
    {
    }
}
