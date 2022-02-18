using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IEmploymentCheckCacheRequestRepository
    {
        Task Save(EmploymentCheckCacheRequest request);
        Task Insert(EmploymentCheckCacheRequest request);
        Task AbandonRelatedRequests(EmploymentCheckCacheRequest request, IUnitOfWork unitOfWork);
        Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest();
    }
}
