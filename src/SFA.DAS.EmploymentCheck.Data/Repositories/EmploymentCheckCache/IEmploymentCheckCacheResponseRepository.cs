using SFA.DAS.EmploymentCheck.Data.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    public interface IEmploymentCheckCacheResponseRepository
    {
        Task Save(EmploymentCheckCacheResponse employmentCheckCacheResponse);
    }
}
