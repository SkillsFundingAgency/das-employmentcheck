using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public interface IEmploymentCheckService
    {
        Task<IList<Apprentice>> GetApprentices();

        Task<int> SaveEmploymentCheckResult(long id, bool result);

        Task<int> SaveEmploymentCheckResult(long id, long uln, bool result);
    }
}
