using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IEmploymentCheckCacheRequestRepository
    {
        Task Save(EmploymentCheckCacheRequest request);

        Task Insert(EmploymentCheckCacheRequest request);

        Task SetReleatedRequestsRequestCompletionStatus(EmploymentCheckCacheRequest request, ProcessingCompletionStatus processingCompletionStatus);
    }
}
