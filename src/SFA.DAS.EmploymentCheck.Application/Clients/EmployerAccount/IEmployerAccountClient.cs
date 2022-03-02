using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Application.Clients.EmployerAccount
{
    public interface IEmployerAccountClient
    {
        Task<EmployerPayeSchemes> GetEmployersPayeSchemes(Models.EmploymentCheck employmentCheck);
    }
}
