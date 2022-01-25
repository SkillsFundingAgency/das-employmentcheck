using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Services.Learner
{
    public interface ILearnerService
    {
        Task<IList<LearnerNiNumber>> GetNiNumbers(IList<Data.Models.EmploymentCheck> employmentCheckBatch);
    }
}
