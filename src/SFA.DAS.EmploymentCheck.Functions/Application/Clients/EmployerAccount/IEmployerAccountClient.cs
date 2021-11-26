using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount
{
    public interface IEmployerAccountClient
    {
        Task<IList<EmployerPayeSchemes>> GetEmployersPayeSchemes(IList<EmploymentCheckModel> apprentices);
    }
}
