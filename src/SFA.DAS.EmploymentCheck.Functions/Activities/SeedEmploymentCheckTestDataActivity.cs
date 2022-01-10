using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Interfaces.PaymentsCompliance;

namespace SFA.DAS.EmploymentCheck.Functions.Activities
{
    public class SeedEmploymentCheckTestDataActivity
    {
        IPaymentsComplianceService _paymentsComplianceService;

        public SeedEmploymentCheckTestDataActivity(
            IPaymentsComplianceService paymentsComplianceService,
            ILogger<SeedEmploymentCheckTestDataActivity> logger)
        {
            _paymentsComplianceService = paymentsComplianceService;
        }

        [FunctionName(nameof(SeedEmploymentCheckTestDataActivity))]
        public async Task<Unit> SeedEmploymentCheckTestDataActivityTask([ActivityTrigger] int input)
        {
            _ = input;

            await _paymentsComplianceService.SeedEmploymentCheckDatabaseTableTestData();
            return await Task.FromResult(Unit.Value);
        }
    }
}
