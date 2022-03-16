using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands
{
    public interface ICommandPublisher
    {
        Task Publish<T>(T command, CancellationToken cancellationToken = default) where T : class, ICommand;
        Task Publish(object command, CancellationToken cancellationToken = default);
    }
}
