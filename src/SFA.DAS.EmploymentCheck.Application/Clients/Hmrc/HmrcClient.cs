﻿using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Services.Hmrc;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Clients.Hmrc
{
    public class HmrcClient : IHmrcClient
    {
        private readonly IHmrcService _hmrcService;
        private readonly ILogger<IHmrcClient> _logger;

        public HmrcClient(IHmrcService  hmrcService, ILogger<HmrcClient> logger)
        {
            _hmrcService = hmrcService;
            _logger = logger;
        }

        public async Task<EmploymentCheckCacheRequest> CheckEmploymentStatus(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            var result = await _hmrcService.IsNationalInsuranceNumberRelatedToPayeScheme(employmentCheckCacheRequest);

            if (result == null)
            {
                _logger.LogError($"{nameof(HmrcClient)}.CheckEmploymentStatus: The employmentCheckMessageResult value returned from the IsNationalInsuranceNumberRelatedToPayeScheme() call returned null.");
            }

            return result;
        }
    }
}