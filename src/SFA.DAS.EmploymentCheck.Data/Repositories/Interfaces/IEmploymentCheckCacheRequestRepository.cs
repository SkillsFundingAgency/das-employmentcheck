using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces
{
    public interface IEmploymentCheckCacheRequestRepository
    {
        Task Save(EmploymentCheckCacheRequest request);
        Task Insert(EmploymentCheckCacheRequest request);
        Task AbandonRelatedRequests(EmploymentCheckCacheRequest request, IUnitOfWork unitOfWork);
        Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest();
    }
}
