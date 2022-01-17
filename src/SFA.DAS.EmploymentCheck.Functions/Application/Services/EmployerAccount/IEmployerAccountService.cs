using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount
{
    public interface IEmployerAccountService
    {
        Task<EmployerPayeSchemes> GetEmployerPayeSchemes(Models.EmploymentCheck employmentChecksBatch);
    }
}
