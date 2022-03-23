using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces
{
    public interface IEmploymentCheckRepository
    {
        Task InsertOrUpdate(Models.EmploymentCheck check);
        Task<Models.EmploymentCheck> GetEmploymentCheck();
        Task<Models.EmploymentCheck> GetResponseEmploymentCheck();
        Task UpdateEmploymentCheckAsComplete(Models.EmploymentCheckCacheRequest request, IUnitOfWork unitOfWork);
        Task UpdateEmploymentCheckAsComplete(Models.EmploymentCheck check, IUnitOfWork unitOfWork);
    }
}