using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount
{
    public class EmployerAccountServiceStub : IEmployerAccountService
    {
        private const string ThisClassName = "\n\nEmployerAccountServiceStub";
        private readonly IEmployerAccountApiClient _accountsApiClient;
        private readonly ILogger<IEmployerAccountService> _logger;

        public EmployerAccountServiceStub(
            IEmployerAccountApiClient accountsApiClient,
            ILogger<IEmployerAccountService> logger)
        {
            _accountsApiClient = accountsApiClient;
            _logger = logger;
        }

        public async Task<ResourceList> GetEmployerAccount(long accountId)
        {
            return await Task.FromResult(new ResourceList(new List<ResourceViewModel>
            {
                new ResourceViewModel
                {
                    Id = (accountId + 1000).ToString(), Href = $"PayeScheme{accountId}"
                }
            }));
        }

        public async Task<AccountDetailViewModel> GetEmployerAccount2(long accountId)
        {
            var thisMethodName = $"{ThisClassName}.GetEmployerAccount()";

            AccountDetailViewModel accountDetailViewModel;
            switch (accountId)
            {
                case 1:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 1,
                        StartingTransferAllowance = 1m,
                        RemainingTransferAllowance = 1m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "1001", Href = "PayeScheme1001" }
                        })
                    };
                    break;

                case 2:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 2,
                        StartingTransferAllowance = 2m,
                        RemainingTransferAllowance = 2m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "2001", Href = "PayeScheme2001" },
                            new ResourceViewModel { Id = "2002", Href = "PayeScheme2002" }
                        })
                    };
                    break;

                case 3:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 3,
                        StartingTransferAllowance = 3m,
                        RemainingTransferAllowance = 3m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "3001", Href = "PayeScheme3001" },
                            new ResourceViewModel { Id = "3002", Href = "PayeScheme3002" },
                            new ResourceViewModel { Id = "3003", Href = "PayeScheme3003" }
                        })
                    };
                    break;

                case 4:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 4,
                        StartingTransferAllowance = 4m,
                        RemainingTransferAllowance = 4m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "4001", Href = "PayeScheme4001" },
                            new ResourceViewModel { Id = "4002", Href = "PayeScheme4002" },
                            new ResourceViewModel { Id = "4003", Href = "PayeScheme4003" },
                            new ResourceViewModel { Id = "4004", Href = "PayeScheme4004" }
                        })
                    };
                    break;

                case 5:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 5,
                        StartingTransferAllowance = 5m,
                        RemainingTransferAllowance = 5m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "5001", Href = "PayeScheme5001" },
                            new ResourceViewModel { Id = "5002", Href = "PayeScheme5002" },
                            new ResourceViewModel { Id = "5003", Href = "PayeScheme5003" },
                            new ResourceViewModel { Id = "5004", Href = "PayeScheme5004" },
                            new ResourceViewModel { Id = "5005", Href = "PayeScheme5005" }
                        })
                    };
                    break;

                case 6:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 6,
                        StartingTransferAllowance = 6m,
                        RemainingTransferAllowance = 6m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "6001", Href = "PayeScheme6001" },
                            new ResourceViewModel { Id = "6002", Href = "PayeScheme6002" },
                            new ResourceViewModel { Id = "6003", Href = "PayeScheme6003" },
                            new ResourceViewModel { Id = "6004", Href = "PayeScheme6004" },
                            new ResourceViewModel { Id = "6005", Href = "PayeScheme6005" },
                            new ResourceViewModel { Id = "6006", Href = "PayeScheme6006" }
                        })
                    };
                    break;

                case 7:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 7,
                        StartingTransferAllowance = 7m,
                        RemainingTransferAllowance = 7m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "7001", Href = "PayeScheme7001" },
                            new ResourceViewModel { Id = "7002", Href = "PayeScheme7002" },
                            new ResourceViewModel { Id = "7003", Href = "PayeScheme7003" },
                            new ResourceViewModel { Id = "7004", Href = "PayeScheme7004" },
                            new ResourceViewModel { Id = "7005", Href = "PayeScheme7005" },
                            new ResourceViewModel { Id = "7006", Href = "PayeScheme7006" },
                            new ResourceViewModel { Id = "7007", Href = "PayeScheme7007" }
                        })
                    };
                    break;

                case 8:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 8,
                        StartingTransferAllowance = 8m,
                        RemainingTransferAllowance = 8m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "8001", Href = "PayeScheme8001" },
                            new ResourceViewModel { Id = "8002", Href = "PayeScheme8002" },
                            new ResourceViewModel { Id = "8003", Href = "PayeScheme8003" },
                            new ResourceViewModel { Id = "8004", Href = "PayeScheme8004" },
                            new ResourceViewModel { Id = "8005", Href = "PayeScheme8005" },
                            new ResourceViewModel { Id = "8006", Href = "PayeScheme8006" },
                            new ResourceViewModel { Id = "8007", Href = "PayeScheme8007" },
                            new ResourceViewModel { Id = "8008", Href = "PayeScheme8008" }
                        })
                    };
                    break;

                case 9:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 9,
                        StartingTransferAllowance = 9m,
                        RemainingTransferAllowance = 9m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "9001", Href = "PayeScheme9001" },
                            new ResourceViewModel { Id = "9002", Href = "PayeScheme9002" },
                            new ResourceViewModel { Id = "9003", Href = "PayeScheme9003" },
                            new ResourceViewModel { Id = "9004", Href = "PayeScheme9004" },
                            new ResourceViewModel { Id = "9005", Href = "PayeScheme9005" },
                            new ResourceViewModel { Id = "9006", Href = "PayeScheme9006" },
                            new ResourceViewModel { Id = "9007", Href = "PayeScheme9007" },
                            new ResourceViewModel { Id = "9008", Href = "PayeScheme9008" },
                            new ResourceViewModel { Id = "9009", Href = "PayeScheme9009" }
                        })
                    };
                    break;

                default:
                    accountDetailViewModel = new AccountDetailViewModel
                    {
                        AccountId = 1,
                        StartingTransferAllowance = 1m,
                        RemainingTransferAllowance = 1m,
                        PayeSchemes = new ResourceList(new List<ResourceViewModel>
                        {
                            new ResourceViewModel { Id = "1001", Href = "PayeScheme1001" }
                        })
                    };
                    break;
            }

            _logger.LogInformation($"{thisMethodName}: returned {accountDetailViewModel.PayeSchemes.Count} PAYE schemes.");
            return await Task.FromResult(accountDetailViewModel);
        }
    }
}

