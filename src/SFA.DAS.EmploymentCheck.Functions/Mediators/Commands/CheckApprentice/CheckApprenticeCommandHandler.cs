using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.DataAccess;
using SFA.DAS.EmploymentCheck.Functions.Dtos;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Services;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice
{
    public class CheckApprenticeCommandHandler : IRequestHandler<CheckApprenticeCommand>
    {
        private readonly IEmploymentChecksRepository _repository;
        private readonly IAccountsService _accountsService;
        private readonly IHmrcService _hmrcService;
        private readonly ILogger<CheckApprenticeCommandHandler> _logger;

        public CheckApprenticeCommandHandler(IEmploymentChecksRepository repository, IAccountsService accountsService, IHmrcService hmrcService, ILogger<CheckApprenticeCommandHandler> logger)
        {
            _repository = repository;
            _accountsService = accountsService;
            _hmrcService = hmrcService;
            _logger = logger;
        }

        public async Task<Unit> Handle(CheckApprenticeCommand request, CancellationToken cancellationToken)
        {
            var thisMethodName = "CheckApprenticeCommandHandler.Handle()";

            try
            {
                var payeSchemes = await GetAccountPayeSchemes(request.Apprentice.AccountId);

                if(payeSchemes != null && payeSchemes.Count > 0)
                {
                    Log.WriteLog(_logger, thisMethodName, $"GetAccountPayeSchemes() returned [{payeSchemes.Count}] PAYE scheme(s) for learner ULN: [{request.Apprentice.ULN}]");
                    bool checkPassed = false;
                    int i = 0;

                    try
                    {
                        foreach (var payeScheme in payeSchemes)
                        {
                            ++i;
                            Log.WriteLog(_logger, thisMethodName, $"Checking scheme number [{i}] of [{payeSchemes.Count}] for scheme name: [{payeScheme}],  StartDate [{request.Apprentice.StartDate}], EndDate [{request.Apprentice.EndDate}]");

                            checkPassed = await _hmrcService.IsNationalInsuranceNumberRelatedToPayeScheme(payeScheme, request, request.Apprentice.StartDate, request.Apprentice.EndDate);

                            if (checkPassed)
                            {
                                break;
                            }
                        }

                        Log.WriteLog(_logger, thisMethodName, $"Saving learner ULN: [{request.Apprentice.ULN}] Employed = [{checkPassed}]");
                        await StoreEmploymentCheckResult(request.Apprentice, checkPassed);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"Exception caught - {ex.Message}. {ex.StackTrace}");
                    }
                }
                else
                {
                    Log.WriteLog(_logger, thisMethodName, $"GetAccountPayeSchemes() returned null/zero PAYE schemes.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return Unit.Value;
        }

        private async Task StoreEmploymentCheckResult(ApprenticeToVerifyDto apprentice, bool checkPassed)
        {
            await _repository.SaveEmploymentCheckResult(apprentice.Id, checkPassed);
        }

        private async Task<List<string>> GetAccountPayeSchemes(long accountId)
        {
            var accountDetail = await _accountsService.GetAccountDetail(accountId);
            return accountDetail.PayeSchemes.Select(x => x.Id).ToList();
        }
    }
}
