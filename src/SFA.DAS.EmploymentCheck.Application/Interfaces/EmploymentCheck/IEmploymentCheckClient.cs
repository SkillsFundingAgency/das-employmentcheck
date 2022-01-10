using SFA.DAS.EmploymentCheck.Domain.Entities;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Interfaces.EmploymentCheck
{
    public interface IEmploymentCheckClient
    {
        Task<EmploymentCheckCacheRequest> CheckEmploymentStatus(EmploymentCheckCacheRequest employmentCheckCacheRequest);
    }
}
