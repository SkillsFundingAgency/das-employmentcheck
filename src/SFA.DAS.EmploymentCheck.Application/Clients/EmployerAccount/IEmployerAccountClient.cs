using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Clients.EmployerAccount
{
    public interface IEmployerAccountClient
    {
        Task<EmployerPayeSchemes> GetEmployersPayeSchemes(Data.Models.EmploymentCheck employmentCheck);
    }
}
