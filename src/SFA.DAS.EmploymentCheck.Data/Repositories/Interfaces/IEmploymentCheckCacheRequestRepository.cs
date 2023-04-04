using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces
{
    public interface IEmploymentCheckCacheRequestRepository
    {
        Task Save(EmploymentCheckCacheRequest request);
        Task Insert(EmploymentCheckCacheRequest request);
        Task AbandonRelatedRequests(EmploymentCheckCacheRequest request, IUnitOfWork unitOfWork);
        Task<EmploymentCheckCacheRequest[]> GetEmploymentCheckCacheRequests();
        Task<List<LearnerPayeCheckPriority>> GetLearnerPayeCheckPriority(string niNumber);
    }
}
