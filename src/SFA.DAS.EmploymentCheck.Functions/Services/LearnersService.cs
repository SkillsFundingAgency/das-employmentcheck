using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Services
{
    public class LearnersService : ILearnersService
    {
        private readonly ILogger<LearnersService> _logger;

        public LearnersService(
            ILogger<LearnersService> logger)
        {
            _logger = logger;
        }

        public async Task<LearnerNationalnsuranceNumberDto[]> GetLearnersNationalInsuranceNumbers(LearnerNationalnsuranceNumberDto[] learnersNinosDto)
        {
            var thisMethodName = $"LearnersService.GetLearnersNationalInsuranceNumbers()";

            // TODO: Implement API call
            try
            {
                //account = await _learnersApiClient.Get<AccountDetailViewModel>($"api/accounts/internal/{accountId}");
                //Log.WriteLog(_logger, thisMethodName, $"Database query returned [0] learners.");
                //_logger.LogInformation($"{messagePrefix} [_accountsApiClient.Get<AccountDetailViewModel>($api/accounts/internal/{accountId})] returned {account.DasAccountName}.");

            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}\n\n Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(learnersNinosDto);
        }

        public Task<List<LearnerRequiringEmploymentCheckDto>> GetLearnersRequiringEmploymentCheck(SqlConnection sqlConnection)
        {
            throw new NotImplementedException();
        }
    }
}