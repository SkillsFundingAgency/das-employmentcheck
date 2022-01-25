using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Clients.EmployerAccount
{
    public interface IEmployerAccountClient
    {
        Task<IList<EmployerPayeSchemes>> GetEmployersPayeSchemes(IList<Data.Models.EmploymentCheck> employmentChecksBatch);
    }
}
