using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces
{
    public interface IEmploymentCheckCacheResponseRepository
    {
        Task Save(EmploymentCheckCacheResponse response);

        Task Insert(EmploymentCheckCacheResponse response);
    }
}
