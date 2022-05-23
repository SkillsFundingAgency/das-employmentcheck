using SFA.DAS.EmploymentCheck.Abstractions;

namespace SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult
{
    public class PublishEmploymentCheckResultCommand : ICommand
    {
        public PublishEmploymentCheckResultCommand(Data.Models.EmploymentCheck employmentCheck)
        {
            EmploymentCheck = employmentCheck;
        }

        public Data.Models.EmploymentCheck EmploymentCheck { get; }
    }
}
