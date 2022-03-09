using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequest
{
    public class CreateEmploymentCheckCacheRequestCommand : ICommand
    {
        public CreateEmploymentCheckCacheRequestCommand(EmploymentCheckData employmentCheckData)
        {
            EmploymentCheckData = employmentCheckData;
        }

        public EmploymentCheckData EmploymentCheckData { get; }
    }
}
