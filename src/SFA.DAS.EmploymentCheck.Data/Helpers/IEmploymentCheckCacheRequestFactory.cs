using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Data.Helpers
{
    public interface IEmploymentCheckCacheRequestFactory
    {
        Task<EmploymentCheckCacheRequest> CreateEmploymentCheckCacheRequest(Models.EmploymentCheck employmentCheck, string nino, string payeScheme);
    }
}
