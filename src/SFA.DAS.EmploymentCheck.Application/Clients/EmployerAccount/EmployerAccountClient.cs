using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Clients.EmployerAccount
{
    public class EmployerAccountClient : IEmployerAccountClient
    {
        private readonly IEmployerAccountService _employerAccountService;

        public EmployerAccountClient(IEmployerAccountService employerAccountService)
        {
            _employerAccountService = employerAccountService;
        }

        public async Task<EmployerPayeSchemes> GetEmployersPayeSchemes(
            Data.Models.EmploymentCheck employmentCheck)
        {
            var result = await _employerAccountService.GetEmployerPayeSchemes(employmentCheck);
         
            return result;
        }
    }
}