using System.Collections.Generic;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IEmploymentCheckRepository
    {
        Task InsertOrUpdate(Models.EmploymentCheck check);
        Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch();
        Task UpdateEmploymentCheckAsComplete(Models.EmploymentCheckCacheRequest request, IUnitOfWork transaction);
    }
}