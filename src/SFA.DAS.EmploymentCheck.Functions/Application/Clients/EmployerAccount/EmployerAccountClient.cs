using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount
{
    public class EmployerAccountClient
        : IEmployerAccountClient
    {
        private readonly IEmployerAccountService _employerAccountService;
        private readonly ILogger<IEmploymentCheckClient> _logger;

        public EmployerAccountClient(
            IEmployerAccountService employerAccountService,
            ILogger<IEmploymentCheckClient> logger)
        {
            _employerAccountService = employerAccountService;
            _logger = logger;
        }

        public async Task<IList<EmployerPayeSchemes>> GetEmployersPayeSchemes(
            IList<ApprenticeEmploymentCheckModel> apprentices)
        {
            var thisMethodName = $"{nameof(EmployerAccountClient)}.GetEmployersPayeSchemes()";

            IList<EmployerPayeSchemes> employerPayeSchemes = new List<EmployerPayeSchemes>();
            try
            {
                if (apprentices != null && apprentices.Count != 0)
                {
                    foreach (var apprentice in apprentices)
                    {
                        Log.WriteLog(_logger, thisMethodName, $"Getting PAYE schemes for employer account [{apprentice.AccountId}] (apprentice ULN [{apprentice.ULN}]).");
                        var payeSchemes = await _employerAccountService.GetEmployerAccount(apprentice.AccountId);

                        if (payeSchemes != null && payeSchemes.Count > 0)
                        {
                            employerPayeSchemes.Add(new EmployerPayeSchemes(apprentice.AccountId, payeSchemes.Select(x => x.Id.ToUpper()).ToList()));
                        }
                        else
                        {
                            _logger.LogInformation($"{thisMethodName}: ERROR: AccountDetailViewModel/PayeSchemes parameter is NULL, no employer PAYE schemes retrieved");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: ERROR: apprentices parameter is NULL, no employer PAYE schemes retrieved");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return employerPayeSchemes;
        }
    }
}
