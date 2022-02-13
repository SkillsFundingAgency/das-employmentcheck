using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Helpers
{
    public interface IEmploymentCheckCacheRequestFactory
    {
        Task<EmploymentCheckCacheRequest> CreateEmploymentCheckCacheRequest(IEmploymentCheck employmentCheck, string nino, string payeScheme);
    }
}
