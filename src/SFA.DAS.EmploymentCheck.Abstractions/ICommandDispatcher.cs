using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Abstractions
{
    public interface ICommandDispatcher
    {
        Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;
    }
}
