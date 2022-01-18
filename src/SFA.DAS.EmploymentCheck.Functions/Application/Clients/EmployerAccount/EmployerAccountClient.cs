using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount
{
    public class EmployerAccountClient : IEmployerAccountClient
    {
        private readonly IEmployerAccountService _employerAccountService;
        public EmployerAccountClient(IEmployerAccountService employerAccountService)
        {
            _employerAccountService = employerAccountService;
        }

        public async Task<IList<EmployerPayeSchemes>> GetEmployersPayeSchemes(
            IList<Models.EmploymentCheck> employmentChecksBatch)
        {
            var employersPayeSchemes = new List<EmployerPayeSchemes>();
            foreach (var employmentCheck in employmentChecksBatch)
            {
                var employerPayeSchemes = await _employerAccountService.GetEmployerPayeSchemes(employmentCheck);
                for (int i = 0; i < employerPayeSchemes.PayeSchemes.Count; i++)
                {
                    employerPayeSchemes.PayeSchemes[i] = employerPayeSchemes.PayeSchemes[i].Trim().ToUpper();
                }
                employersPayeSchemes.Add(employerPayeSchemes);
            }

            return employersPayeSchemes;
        }
    }
}