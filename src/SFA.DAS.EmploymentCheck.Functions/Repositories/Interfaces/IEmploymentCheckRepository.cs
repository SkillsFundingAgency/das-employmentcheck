using System.Collections.Generic;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IEmploymentCheckRepository
    {
        Task<long> Save(Application.Models.EmploymentCheck check);

        Task<long> Insert(Models.EmploymentCheck check);

        Task<long> UpdateEmployedAndRequestStatusFields(Models.EmploymentCheck check);

        Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch();
    }
}