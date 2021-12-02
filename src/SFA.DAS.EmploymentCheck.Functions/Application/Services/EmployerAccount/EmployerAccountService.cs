using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using System;
using System.Threading.Tasks;
using SFA.DAS.HashingService;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount
{
    public class EmployerAccountService : IEmployerAccountService
    {
        private readonly IEmployerAccountApiClient _accountsApiClient;
        private readonly IHashingService _hashingService;
        private readonly ILogger<IEmployerAccountService> _logger;

        public EmployerAccountService(
            IEmployerAccountApiClient accountsApiClient,
            IHashingService hashingService,
            ILogger<IEmployerAccountService> logger)
        {
            _accountsApiClient = accountsApiClient;
            _hashingService = hashingService;
            _logger = logger;
        }

        public async Task<AccountDetailViewModel> GetEmployerAccount(long accountId) // TODO: rename to GetAccountPayeSchemes
        {
            var thisMethodName = $"{nameof(EmployerAccountService)}.GetEmployerAccount()";

            AccountDetailViewModel accountDetailViewModel = null;
            try
            {
                accountDetailViewModel = await _accountsApiClient.Get<AccountDetailViewModel>($"api/accounts/{_hashingService.HashValue(accountId)}/payeschemes");
                if (accountDetailViewModel != null && accountDetailViewModel.PayeSchemes?.Count > 0)
                {
                    _logger.LogInformation($"{thisMethodName}: returned {accountDetailViewModel.PayeSchemes.Count} PAYE schemes");
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: returned null/zero PAYE schemes");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}\n\n Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return accountDetailViewModel;
        }
    }
}
