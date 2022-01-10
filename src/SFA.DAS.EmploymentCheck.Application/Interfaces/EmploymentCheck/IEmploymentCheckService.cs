using SFA.DAS.EmploymentCheck.Domain.Entities;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Interfaces.EmploymentCheck
{
    public interface IEmploymentCheckService
    {
        Task<EmploymentCheckCacheRequest> IsNationalInsuranceNumberRelatedToPayeScheme(EmploymentCheckCacheRequest request);
    }
}
