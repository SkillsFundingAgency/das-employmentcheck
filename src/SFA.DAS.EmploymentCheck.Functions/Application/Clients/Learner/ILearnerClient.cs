using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.Learner
{
    public interface ILearnerClient
    {
        Task<IList<LearnerNiNumber>> GetNiNumbers(IList<Models.EmploymentCheck> apprentices);
    }
}
