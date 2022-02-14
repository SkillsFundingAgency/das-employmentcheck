using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IEmploymentCheckCacheRequestRepository
    {
        Task Save(EmploymentCheckCacheRequest request);
        Task Insert(EmploymentCheckCacheRequest request);
        Task SetRelatedRequestsRequestCompletionStatus(EmploymentCheckCacheRequest request, ProcessingCompletionStatus processingCompletionStatus);
        Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest();
    }
}
