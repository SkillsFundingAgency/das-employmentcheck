using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Learner
{
    public interface ILearnerService
    {
        Task<IList<LearnerNiNumber>> GetNiNumbers(IList<Models.EmploymentCheck> employmentCheckBatch);

        Task<int> StoreDataCollectionsResponse(DataCollectionsResponse dataCollectionsResponse);
    }
}
