using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;
using SFA.DAS.EmploymentCheck.Functions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmployerPayeSchemes
{
    public class GetEmployerPayeSchemesHandler
        : IRequestHandler<GetEmployerPayeSchemesRequest,
            GetEmployerPayeSchemesResult>
    {
        private readonly IAccountsService _accountsService;
        private ILogger<GetEmployerPayeSchemesHandler> _logger;

        public GetEmployerPayeSchemesHandler(
            IAccountsService accountsService,
            ILogger<GetEmployerPayeSchemesHandler> logger)
        {
            _accountsService = accountsService;
            _logger = logger;
        }

        public async Task<GetEmployerPayeSchemesResult> Handle(
            GetEmployerPayeSchemesRequest getEmployerPayeSchemesRequest,
            CancellationToken cancellationToken)
        {
            var thisMethodName = "GetEmployerPayeSchemesResult.Handle(): ";

            GetEmployerPayeSchemesResult getEmployerPayeSchemesResult = new GetEmployerPayeSchemesResult(new List<EmployerPayeSchemesDto>());
            try
            {
                // call the application methods with the application types instead of the medaiatr request wrapper type
                getEmployerPayeSchemesResult = await GetEmployerPayeSchemes(getEmployerPayeSchemesRequest.EmployerPayeSchemesDtos);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}\n\nException caught - {ex.Message}. {ex.StackTrace}");
            }

            return getEmployerPayeSchemesResult;
        }

        private async Task<GetEmployerPayeSchemesResult> GetEmployerPayeSchemes(IList<EmployerPayeSchemesDto> employerPayeSchemesDtos)
        {
            var thisMethodName = "GetEmployerPayeSchemesHandler.GetEmployerPayeSchemes(): ";

            GetEmployerPayeSchemesResult getEmployerPayeSchemesResult = new GetEmployerPayeSchemesResult(new List<EmployerPayeSchemesDto>());

            try
            {
                if (employerPayeSchemesDtos != null && employerPayeSchemesDtos.Count > 0)
                {
                    foreach (var employerPayeSchemesDto in employerPayeSchemesDtos)
                    {
                        try
                        {
                            var employerPayeSchemeDto = await GetAccountDetail(employerPayeSchemesDto);
                            if (employerPayeSchemeDto != null)
                            {
                                getEmployerPayeSchemesResult.EmployerPayeSchemesDtos.Add(employerPayeSchemeDto);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation($"{thisMethodName} *** Exception caught - FAILED TO RETRIEVE PAYE SCHEMES FOR ACCOUNT ID {employerPayeSchemesDto.AccountId} *** \n\n{ex.Message}. {ex.StackTrace}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            if (getEmployerPayeSchemesResult.EmployerPayeSchemesDtos != null && getEmployerPayeSchemesResult.EmployerPayeSchemesDtos.Count > 0)
            {
                Log.WriteLog(_logger, thisMethodName, $"returned {getEmployerPayeSchemesResult.EmployerPayeSchemesDtos.Count} paye schemes(s).");
            }
            else
            {
                Log.WriteLog(_logger, thisMethodName, $"RETURNED NULL/ZERO PAYE SCHEMES.");
            }

            return getEmployerPayeSchemesResult;
        }

        private async Task<EmployerPayeSchemesDto> GetAccountDetail(
            EmployerPayeSchemesDto employerPayeSchemesDto)
        {
            var thisMethodName = "GetEmployerPayeSchemesHandler.GetAccountDetail(): ";

            try
            {
                if(employerPayeSchemesDto != null)
                {
                    var accountDetailViewModel = await _accountsService.GetAccountDetail(employerPayeSchemesDto.AccountId);

                    if (accountDetailViewModel != null && accountDetailViewModel.PayeSchemes != null && accountDetailViewModel.PayeSchemes.Count > 0)
                    {
                        Log.WriteLog(_logger, thisMethodName, $"returned {accountDetailViewModel.PayeSchemes.Count} paye schemes(s).");

                        employerPayeSchemesDto.PayeSchemes = accountDetailViewModel.PayeSchemes.Select(x => x.Id).ToList();
                    }
                    else
                    {
                        Log.WriteLog(_logger, thisMethodName, $"RETURNED NULL/ZERO PAYE SCHEMES.");
                    }
                }
                else
                {
                    Log.WriteLog(_logger, thisMethodName, $"*** EmployerPayeSchemeDto input parameter is null ***");
                }
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}\n\nException caught - {ex.Message}. {ex.StackTrace}");
            }

            return employerPayeSchemesDto;
        }
    }
}