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
            EmployerPayeSchemes payeSchemes = null;
            if (employmentCheck != null && employmentCheck.Id != 0)
            {
                payeSchemes = await _employerAccountService.GetEmployerPayeSchemes(employmentCheck);
                if(payeSchemes != null && payeSchemes.PayeSchemes != null)
                {
                    for(int i = 0; i < payeSchemes.PayeSchemes.Count; ++i)
                    {
                        payeSchemes.PayeSchemes[i] = payeSchemes.PayeSchemes[i].ToUpper();
                    }
                }
            }

            return payeSchemes ?? new EmployerPayeSchemes();
        }
    }
}