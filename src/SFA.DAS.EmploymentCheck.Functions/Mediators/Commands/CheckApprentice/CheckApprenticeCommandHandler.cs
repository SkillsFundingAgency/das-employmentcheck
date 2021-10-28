﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice
{
    public class CheckApprenticeCommandHandler : IRequestHandler<CheckApprenticeCommand>
    {
        private readonly IEmploymentCheckService _employmentCheckService;
        private readonly IEmployerAccountService _accountsService;
        private readonly IHmrcService _hmrcService;
        private readonly ILoggerAdapter<CheckApprenticeCommandHandler> _logger;

        public CheckApprenticeCommandHandler(
            IEmploymentCheckService employmentCheckService,
            IEmployerAccountService accountsService,
            IHmrcService hmrcService,
            ILoggerAdapter<CheckApprenticeCommandHandler> logger)
        {
            _employmentCheckService = employmentCheckService;
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
                            Log.WriteLog(_logger, thisMethodName, $"Checking learner ULN: [{request.Apprentice.ULN}] is on PAYE scheme name [{payeScheme}] for the period [{request.Apprentice.StartDate}] to [{request.Apprentice.EndDate}] (scheme [{i}] of [{payeSchemes.Count}])");

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
                    //Log.WriteLog(_logger, thisMethodName, $"GetAccountPayeSchemes() returned null/zero PAYE schemes.");
                    _logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: GetAccountPayeSchemes() returned null/zero PAYE schemes.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return Unit.Value;
        }

        private async Task StoreEmploymentCheckResult(Apprentice apprentice, bool checkPassed)
        {
            await _employmentCheckService.SaveEmploymentCheckResult(apprentice.Id, apprentice.ULN, checkPassed);
        }

        private async Task<List<string>> GetAccountPayeSchemes(long employerAccountId)
        {
            var accountDetail = await _accountsService.GetEmployerAccount(employerAccountId);
            return accountDetail.PayeSchemes.Select(x => x.Id).ToList();
        }
    }
}
