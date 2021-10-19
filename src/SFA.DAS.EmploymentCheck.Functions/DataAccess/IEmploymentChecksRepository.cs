using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.DataAccess
{
    public interface IEmploymentChecksRepository
    {
        Task<List<ApprenticeToVerifyDto>> GetApprenticesToCheck();

        Task<int> SaveEmploymentCheckResult(long id, bool result);
    }
}
