using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.DataAccess
{
    public interface IEmploymentChecksRepository
    {
        Task<List<ApprenticeToVerifyDto>> GetApprenticesToCheck();
        Task SaveEmploymentCheckResult(long id, bool result);
    }
}
