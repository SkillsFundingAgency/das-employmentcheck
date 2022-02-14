using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Helpers
{
    public interface IEmploymentCheckCacheRequestFactory
    {
        Task<EmploymentCheckCacheRequest> CreateEmploymentCheckCacheRequest(Models.EmploymentCheck employmentCheck, string nino, string payeScheme);
    }
}
