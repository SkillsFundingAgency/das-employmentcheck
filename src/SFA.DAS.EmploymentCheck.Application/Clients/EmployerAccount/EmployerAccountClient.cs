using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Common.Models;
using SFA.DAS.EmploymentCheck.Application.Interfaces.EmployerAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Clients.EmployerAccount
{
    public class EmployerAccountClient
        : IEmployerAccountClient
    {
        #region Private members
        private readonly ILogger<IEmployerAccountClient> _logger;
        private readonly IEmployerAccountService _employerAccountService;

        #endregion Private members

        #region Constructors
        public EmployerAccountClient(
            ILogger<IEmployerAccountClient> logger,
            IEmployerAccountService employerAccountService
            )
        {
            _employerAccountService = employerAccountService;
            _logger = logger;
        }
        #endregion Constructors

        #region GetEmployersPayeSchemes
        public async Task<IList<EmployerPayeSchemes>> GetEmployersPayeSchemes(
            IList<Domain.Entities.EmploymentCheck> apprenticeEmploymentChecks)
        {
            var thisMethodName = $"{nameof(EmployerAccountClient)}.GetEmployersPayeSchemes";

            IList<EmployerPayeSchemes> employerPayeSchemes = new List<EmployerPayeSchemes>();
            try
            {
                if (apprenticeEmploymentChecks != null && apprenticeEmploymentChecks.Count != 0)
                {
                    foreach (var apprenticeEmploymentCheck in apprenticeEmploymentChecks)
                    {
                        _logger.LogInformation($"{thisMethodName}: Getting PAYE scheme for employer account [{apprenticeEmploymentCheck.AccountId}] (apprentice ULN [{apprenticeEmploymentCheck.Uln}]).");
                        var resourceList = await _employerAccountService.GetPayeSchemes(apprenticeEmploymentCheck);

                        if (resourceList != null && resourceList.Any())
                        {
                            employerPayeSchemes.Add(new EmployerPayeSchemes(apprenticeEmploymentCheck.AccountId, resourceList.Select(x => x.Id).ToList()));
                        }
                        else
                        {
                            _logger.LogInformation($"{thisMethodName}: ERROR: resourceList parameter is NULL, no employer PAYE schemes retrieved");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: ERROR: the resourceList input parameter is NULL, no employer PAYE schemes retrieved");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return employerPayeSchemes;
        }
        #endregion GetEmployersPayeSchemes
    }
}