using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount
{
    public class EmployerAccountService : IEmployerAccountService
    {
        private readonly IEmployerAccountApiClient _accountsApiClient;
        private readonly ILogger<IEmployerAccountService> _logger;
        private readonly AccountsApiSettings _accountsApiSettings;

        public EmployerAccountService(
            IEmployerAccountApiClient accountsApiClient,
            ILogger<IEmployerAccountService> logger, IOptions<AccountsApiSettings> accountsApiSettings)
        {
            _accountsApiClient = accountsApiClient;
            _logger = logger;
            _accountsApiSettings = accountsApiSettings.Value;
        }

        public async Task<AccountDetailViewModel> GetEmployerAccount(long accountId)
        {
            var thisMethodName = "AccountsService.GetAccountDetail()";

            AccountDetailViewModel accountDetailViewModel = null;
            try
            {
                accountDetailViewModel = await _accountsApiClient.Get<AccountDetailViewModel>($"api/accounts/internal/{accountId})");
                if(accountDetailViewModel != null && accountDetailViewModel.PayeSchemes != null && accountDetailViewModel.PayeSchemes.Count > 0)
                {
                    _logger.LogInformation($"{thisMethodName}: returned {accountDetailViewModel.PayeSchemes.Count} PAYE schemes");
                }
                {
                    _logger.LogInformation($"{thisMethodName}: returned null/zero PAYE schemes");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}\n\n Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return accountDetailViewModel;
        }
    }
}
