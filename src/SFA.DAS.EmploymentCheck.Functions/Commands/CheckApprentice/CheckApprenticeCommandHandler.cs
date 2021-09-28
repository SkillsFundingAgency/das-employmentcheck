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
        private readonly ILogger _logger;

        public CheckApprenticeCommandHandler(IEmploymentChecksRepository repository, IAccountsService accountsService, IHmrcService hmrcService, ILogger logger)
        {
            _repository = repository;
            _accountsService = accountsService;
            _hmrcService = hmrcService;
            _logger = logger;
        }

        public async Task<Unit> Handle(CheckApprenticeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var payeSchemes = await GetAccountPayeSchemes(request.Apprentice.AccountId);
                bool checkPassed = false;
                foreach (var payeScheme in payeSchemes)
                {
                    checkPassed = await _hmrcService.IsNationalInsuranceNumberRelatedToPayeScheme(payeScheme, request.Apprentice.NationalInsuranceNumber, request.Apprentice.StartDate, request.Apprentice.EndDate);
                    if (checkPassed)
                    {
                        break;
                    }
                }

                await StoreEmploymentCheckResult(request.Apprentice, checkPassed);
                return Unit.Value;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error handling CheckApprenticeCommand");
                throw;
            }
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
