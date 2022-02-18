using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount
{
    public interface IEmployerAccountClient
    {
        Task<EmployerPayeSchemes> GetEmployersPayeSchemes(Models.EmploymentCheck employmentCheck);
    }
}
