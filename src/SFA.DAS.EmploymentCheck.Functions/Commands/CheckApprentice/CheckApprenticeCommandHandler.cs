using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.DataAccess;
using SFA.DAS.EmploymentCheck.Functions.Dtos;
using SFA.DAS.EmploymentCheck.Functions.Services;

namespace SFA.DAS.EmploymentCheck.Functions.Commands.CheckApprentice
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
            var thisMethodName = "***** CheckApprenticeCommandHandler.Handle(CheckApprenticeCommand request, CancellationToken cancellationToken) *****";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            try
            {
                _logger.LogInformation($"{messagePrefix} Executing GetAccountPayeSchemes() for apprentice {request.Apprentice.ULN}, account {request.Apprentice.AccountId}.");
                var payeSchemes = await GetAccountPayeSchemes(request.Apprentice.AccountId);

                if(payeSchemes != null && payeSchemes.Count > 0)
                {
                    _logger.LogInformation($"{messagePrefix} GetAccountPayeSchemes() returned {payeSchemes.Count} PAYE schemes.");

                    bool checkPassed = false;
                    int i = 0;

                    try
                    {
                        foreach (var payeScheme in payeSchemes)
                        {
                            ++i;
                            _logger.LogInformation($"{messagePrefix} Scheme {i} of {payeSchemes.Count} Executing HMRC API [IsNationalInsuranceNumberRelatedToPayeScheme] with Apprentice Id {request.Apprentice.Id}, PAYE Scheme {payeScheme}, StartDate {request.Apprentice.StartDate}, EndDate {request.Apprentice.EndDate}");

                            checkPassed = await _hmrcService.IsNationalInsuranceNumberRelatedToPayeScheme(payeScheme, request, request.Apprentice.StartDate, request.Apprentice.EndDate);
                            _logger.LogInformation($"{messagePrefix} HMRC API [IsNationalInsuranceNumberRelatedToPayeScheme] returned {checkPassed}");

                            if (checkPassed)
                            {
                                break;
                            }
                        }

                        _logger.LogInformation($"{messagePrefix} Executing StoreEmploymentCheckResult().");
                        await StoreEmploymentCheckResult(request.Apprentice, checkPassed);
                        _logger.LogInformation($"{messagePrefix} Executing StoreEmploymentCheckResult() completed.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
                    }
                }
                else
                {
                    _logger.LogInformation($"{messagePrefix} GetAccountPayeSchemes() returned null/zero PAYE schemes.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            _logger.LogInformation($"{messagePrefix} Completed.");
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
