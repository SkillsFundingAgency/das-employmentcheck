using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount
{
    public class EmployerAccountServiceStub : IEmployerAccountService
    {
        private readonly IEmployerAccountApiClient _accountsApiClient;
        private readonly ILogger<IEmployerAccountService> _logger;

        public EmployerAccountServiceStub(
            IEmployerAccountApiClient accountsApiClient,
            ILogger<IEmployerAccountService> logger)
        {
            _accountsApiClient = accountsApiClient;
            _logger = logger;
        }

        public async Task<AccountDetailViewModel> GetEmployerAccount(long accountId)
        {
            var thisMethodName = "EmployerAccountServiceStub.GetEmployerAccount(): ";

            var payeSchemesResourceViewModel = new ResourceViewModel();
            payeSchemesResourceViewModel.Id = "1";
            payeSchemesResourceViewModel.Href = "";

            var paySchemesResourceViewModelList = new List<ResourceViewModel>();
            paySchemesResourceViewModelList.Add(payeSchemesResourceViewModel);

            var accountDetailViewModel = new AccountDetailViewModel
            {
                AccountId = 1,
                StartingTransferAllowance = 10m,
                RemainingTransferAllowance = 5m,
                PayeSchemes = new ResourceList(paySchemesResourceViewModelList)
            };

            Log.WriteLog(_logger, thisMethodName, $"returned {accountDetailViewModel.PayeSchemes.Count} PAYE schemes.");
            return await Task.FromResult(accountDetailViewModel);
        }
    }
}

