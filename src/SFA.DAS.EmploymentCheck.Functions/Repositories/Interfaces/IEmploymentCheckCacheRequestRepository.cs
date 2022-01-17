using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IEmploymentCheckCacheRequestRepository
    {
        Task Save(EmploymentCheckCacheRequest employmentCheckCacheRequest);
    }
}
