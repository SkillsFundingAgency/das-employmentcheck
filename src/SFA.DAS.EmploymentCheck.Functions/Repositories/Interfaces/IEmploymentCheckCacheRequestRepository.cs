using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IEmploymentCheckCacheRequestRepository
    {
        Task InsertOrUpdate(EmploymentCheckCacheRequest request);

        Task Save(EmploymentCheckCacheRequest request);

        Task SkipEmploymentChecksForReleatedEmploymentCheckCacheRequests(EmploymentCheckCacheRequest request);
    }
}
