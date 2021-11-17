using System;
using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Client;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
{
    public class HmrcService : IHmrcService
    {
        private IApprenticeshipLevyApiClient _apprenticeshipLevyService;
        private readonly ILogger<HmrcService> _logger;
        private ITokenServiceApiClient _tokenService;

        public HmrcService(ITokenServiceApiClient tokenService, IApprenticeshipLevyApiClient apprenticeshipLevyService, ILogger<HmrcService> logger)
        {
            _tokenService = tokenService;
            _apprenticeshipLevyService = apprenticeshipLevyService;
            _logger = logger;
        }

        public Task<ApprenticeEmploymentCheckMessageModel> IsNationalInsuranceNumberRelatedToPayeScheme(ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel)
        {
            throw new NotImplementedException();
        }
    }
}
