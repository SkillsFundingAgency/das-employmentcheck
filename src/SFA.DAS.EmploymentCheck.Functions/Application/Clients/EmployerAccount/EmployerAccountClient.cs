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
        private readonly ILogger<IEmploymentCheckClient> _logger;
        private readonly IEmployerAccountService _employerAccountService;
        public EmployerAccountClient(
            ILogger<IEmploymentCheckClient> logger,
            IEmployerAccountService employerAccountService
            )
        {
            _employerAccountService = employerAccountService;
            _logger = logger;
        }

        public async Task<IList<EmployerPayeSchemes>> GetEmployersPayeSchemes(
            IList<Models.EmploymentCheck> apprenticeEmploymentChecks)
        {
            var thisMethodName = $"{nameof(EmployerAccountClient)}.GetEmployersPayeSchemes";

            var employerPayeSchemes = new List<EmployerPayeSchemes>();

            if (apprenticeEmploymentChecks != null && apprenticeEmploymentChecks.Any())
            {
                foreach (var apprenticeEmploymentCheck in apprenticeEmploymentChecks)
                {
                    var resourceList = await _employerAccountService.GetPayeSchemes(apprenticeEmploymentCheck);

                    if (resourceList != null && resourceList.Any())
                    {
                        employerPayeSchemes.Add(new EmployerPayeSchemes(apprenticeEmploymentCheck.AccountId,
                            resourceList.Select(x => x.Id).ToList()));
                    }
                    else
                    {
                        _logger.LogError($"{thisMethodName}: ERROR: resourceList parameter is NULL, no employer PAYE schemes retrieved");
                    }
                }
            }
            else
            {
                _logger.LogError($"{thisMethodName}: ERROR: the resourceList input parameter is NULL, no employer PAYE schemes retrieved");
            }

            return employerPayeSchemes;
        }
    }
}