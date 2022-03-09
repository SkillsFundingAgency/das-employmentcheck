using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands
{
    public interface ICommandHandler<in T> where T: ICommand
    {
        Task Handle(T command, CancellationToken cancellationToken = default);
    }
}
