using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount
{
    public interface IEmployerAccountService
    {
        Task<EmployerPayeSchemes> GetEmployerPayeSchemes(Models.EmploymentCheck employmentCheck);
    }
}
