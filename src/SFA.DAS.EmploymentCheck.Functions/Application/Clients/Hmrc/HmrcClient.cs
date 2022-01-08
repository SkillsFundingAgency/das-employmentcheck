using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.Hmrc
{
    public class HmrcClient : IHmrcClient
    {
        private readonly IHmrcService _hmrcService;

        public HmrcClient(IHmrcService  hmrcService)
        {
            _hmrcService = hmrcService;
        }

        public async Task<EmploymentCheckCacheRequest> CheckEmploymentStatus(EmploymentCheckCacheRequest request)
        {
            return await _hmrcService.IsNationalInsuranceNumberRelatedToPayeScheme(request);
        }
    }
}
