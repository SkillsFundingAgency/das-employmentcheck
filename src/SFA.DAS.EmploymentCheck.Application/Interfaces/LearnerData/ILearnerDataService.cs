using SFA.DAS.EmploymentCheck.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Interfaces.LearnerData
{
    public interface ILearnerDataService
    {
        Task<IList<LearnerNiNumber>> GetNiNumbers(IList<Domain.Entities.EmploymentCheck> employmentCheckBatch);

        Task<int> StoreDataCollectionsResponse(DataCollectionsResponse dataCollectionsResponse);
    }
}
