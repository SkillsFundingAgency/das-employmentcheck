using SFA.DAS.EmploymentCheck.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Interfaces.EmployerAccount
{
    public interface IEmployerAccountClient
    {
        Task<IList<EmployerPayeSchemes>> GetEmployersPayeSchemes(IList<Domain.Entities.EmploymentCheck> apprenticeEmploymentChecks);
    }
}
