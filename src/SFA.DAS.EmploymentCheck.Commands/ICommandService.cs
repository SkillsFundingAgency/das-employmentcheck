using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands
{
    public interface ICommandService
    {
        Task Dispatch<T>(T command) where T : DomainCommand;
    }
}
