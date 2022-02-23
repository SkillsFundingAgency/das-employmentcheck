﻿using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
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

        public async Task<EmployerPayeSchemes> GetEmployersPayeSchemes(
            Models.EmploymentCheck employmentCheck)
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