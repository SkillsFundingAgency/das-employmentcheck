using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class SeedEmploymentCheckTestDataActivity
    {
        #region Private members
        private const string ThisClassName = "\n\nEmploymentCheckServiceBase";
        IEmploymentCheckService _employmentCheckService;
        private readonly ILogger<SeedEmploymentCheckTestDataActivity> _logger;
        #endregion Private members

        #region Constructors
        public SeedEmploymentCheckTestDataActivity(
            IEmploymentCheckService employmentCheckService,
            ILogger<SeedEmploymentCheckTestDataActivity> logger)
        {
            _employmentCheckService = employmentCheckService;
            _logger = logger;
        }
        #endregion Constructors

        #region SeedEmploymentCheckTestDataActivityTask
        [FunctionName(nameof(SeedEmploymentCheckTestDataActivity))]
        public async Task<int> SeedEmploymentCheckTestDataActivityTask(
            [ActivityTrigger] int input)
        {
            var thisMethodName = $"\n\n{ThisClassName}.SeedEmploymentCheckTestDataActivityTask()";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            try
            {
                await _employmentCheckService.SeedEmploymentCheckDatabaseTableTestData();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(0);
        }
        #endregion SeedEmploymentCheckTestDataActivityTask
    }
}
