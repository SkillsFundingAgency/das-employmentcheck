using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class CreateRateLimiterOptionsActivity
    {
        private readonly IHmrcApiOptionsRepository _optionsRepository;
        private readonly ILogger<CreateRateLimiterOptionsActivity> _logger;

        public CreateRateLimiterOptionsActivity(
            IHmrcApiOptionsRepository optionsRepository,
            ILogger<CreateRateLimiterOptionsActivity> logger)
        {
            _optionsRepository = optionsRepository;
            _logger = logger;
        }

        [FunctionName(nameof(CreateRateLimiterOptionsActivity))]
        public async Task CreateRateLimiterOptionsActivityTask([ActivityTrigger] string instance)
        {
            var thisMethodName = $"{nameof(CreateRateLimiterOptionsActivity)}.CreateRateLimiterOptionsActivityTask()";

            try
            {
                await _optionsRepository.CreateDefaultOptions();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName} Exception caught - {ex.Message}. {ex.StackTrace}");
                throw;
            }
        }
    }
}
