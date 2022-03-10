using Models = SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Commands.StoreCompletedEmploymentCheck
{
    public class StoreCompletedEmploymentCheckCommand : ICommand
    {
        public StoreCompletedEmploymentCheckCommand(Models.EmploymentCheck employmentCheck)
        {
            EmploymentCheck = employmentCheck;
        }

        public Models.EmploymentCheck EmploymentCheck { get; }
    }
}
