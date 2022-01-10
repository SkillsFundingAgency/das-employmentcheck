using SFA.DAS.EmploymentCheck.Application.Interfaces.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Clients.EmploymentCheck
{
    public class EmploymentCheckClient : IEmploymentCheckClient
    {
        private readonly IEmploymentCheckService _hmrcService;

        public EmploymentCheckClient(IEmploymentCheckService  hmrcService)
        {
            _hmrcService = hmrcService;
        }

        public async Task<EmploymentCheckCacheRequest> CheckEmploymentStatus(EmploymentCheckCacheRequest request)
        {
            return await _hmrcService.IsNationalInsuranceNumberRelatedToPayeScheme(request);
        }
    }
}
