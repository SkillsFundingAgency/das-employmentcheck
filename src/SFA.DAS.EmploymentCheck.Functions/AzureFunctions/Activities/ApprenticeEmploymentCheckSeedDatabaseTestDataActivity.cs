using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class ApprenticeEmploymentCheckSeedDatabaseTestDataActivity
    {
        private const string ThisClassName = "\n\nEmploymentCheckServiceBase";
        IEmploymentCheckService _employmentCheckService;
        private readonly ILogger<ApprenticeEmploymentCheckSeedDatabaseTestDataActivity> _logger;

        public ApprenticeEmploymentCheckSeedDatabaseTestDataActivity(
            IEmploymentCheckService employmentCheckService,
            ILogger<ApprenticeEmploymentCheckSeedDatabaseTestDataActivity> logger)
        {
            _employmentCheckService = employmentCheckService;
            _logger = logger;
        }

        [FunctionName(nameof(ApprenticeEmploymentCheckSeedDatabaseTestDataActivity))]
        public async Task<int> EmploymentCheckApprenticesSeedDatabaseTestDataActivityTask(
            [ActivityTrigger] int input)
        {
            var thisMethodName = $"\n\n{ThisClassName}.EmploymentCheckApprenticesSeedDatabaseTestDataActivityTask()";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            try
            {
                await _employmentCheckService.SeedEmploymentCheckApprenticeDatabaseTableTestData();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(0);
        }
    }
}
