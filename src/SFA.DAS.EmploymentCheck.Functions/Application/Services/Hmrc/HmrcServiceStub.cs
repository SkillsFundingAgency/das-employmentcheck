using HMRC.ESFA.Levy.Api.Client;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice;
using SFA.DAS.EmploymentCheck.Functions.Services;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
{
    public class HmrcServiceStub : IHmrcService
    {
        private IRandomNumberService _randomNumberService;
        private readonly ILoggerAdapter<IApprenticeshipLevyApiClient> _logger;

        public HmrcServiceStub(
            IRandomNumberService randomNumberService,
            ILoggerAdapter<IApprenticeshipLevyApiClient> logger)
        {
            _randomNumberService = randomNumberService;
            _logger = logger;
        }

        public async Task<bool> IsNationalInsuranceNumberRelatedToPayeScheme(string payeScheme, CheckApprenticeCommand checkApprenticeCommand, DateTime startDate, DateTime endDate)
        {
            //var thisMethodName = $"HmrcServiceStub.IsNationalInsuranceNumberRelatedToPayeScheme()";
            //var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            var employmentStatus = _randomNumberService.GetRandomBool();

            //_logger.LogInformation($"{messagePrefix} IsNationalInsuranceNumberRelatedToPayeScheme() returned {employmentStatus}");

            return await Task.FromResult(employmentStatus);
        }
    }
}
