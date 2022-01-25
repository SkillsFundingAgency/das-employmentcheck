using SFA.DAS.EmploymentCheck.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount;

namespace SFA.DAS.EmploymentCheck.Application.Clients.EmployerAccount
{
    public class EmployerAccountClient : IEmployerAccountClient
    {
        private readonly IEmployerAccountService _employerAccountService;

        public EmployerAccountClient(IEmployerAccountService employerAccountService)
        {
            _employerAccountService = employerAccountService;
        }

        public async Task<IList<EmployerPayeSchemes>> GetEmployersPayeSchemes(
            IList<Data.Models.EmploymentCheck> employmentChecksBatch)
        {
            var employersPayeSchemes = new List<EmployerPayeSchemes>();
            foreach (var employmentCheck in employmentChecksBatch)
            {
                var employerPayeSchemes = await _employerAccountService.GetEmployerPayeSchemes(employmentCheck);
                employersPayeSchemes.Add(employerPayeSchemes);
            }

            return employersPayeSchemes;
        }
    }
}