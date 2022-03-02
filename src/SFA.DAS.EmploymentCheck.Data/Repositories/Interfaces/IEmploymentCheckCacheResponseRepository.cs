using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces
{
    public interface IEmploymentCheckCacheResponseRepository
    {
        Task Save(EmploymentCheckCacheResponse response);

        Task Insert(EmploymentCheckCacheResponse response);
    }
}
