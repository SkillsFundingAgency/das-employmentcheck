using SFA.DAS.EmploymentCheck.Domain.Entities;
using SFA.DAS.EmploymentCheck.Domain.Common.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Interfaces.PaymentsCompliance
{
    public interface IPaymentsComplianceService
    {
        Task<IList<Domain.Entities.EmploymentCheck>> GetEmploymentChecksBatch();

        Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(EmploymentCheckData employmentCheckData);

        Task<EmploymentCheckCacheRequest> GetNextEmploymentCheckCacheRequest();

        Task StoreEmploymentCheckResult(EmploymentCheckCacheRequest employmentCheckCacheRequest);

        Task SeedEmploymentCheckDatabaseTableTestData();
    }
}