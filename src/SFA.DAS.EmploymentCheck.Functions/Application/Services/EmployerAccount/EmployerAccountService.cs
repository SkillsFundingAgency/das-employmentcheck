﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount
{
    public class EmployerAccountService : IEmployerAccountService
    {
        private readonly IEmployerAccountApiClient _accountsApiClient;
        private ILoggerAdapter<IEmployerAccountService> _logger;

        public EmployerAccountService(
            IEmployerAccountApiClient accountsApiClient,
            ILoggerAdapter<IEmployerAccountService> logger)
        {
            _accountsApiClient = accountsApiClient;
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
                    Log.WriteLog(_logger, thisMethodName, $"returned {accountDetailViewModel.PayeSchemes.Count} PAYE schemes.");
                }
                {
                    Log.WriteLog(_logger, thisMethodName, $"GetAccountPayeSchemes() returned null/zero PAYE schemes.");
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
