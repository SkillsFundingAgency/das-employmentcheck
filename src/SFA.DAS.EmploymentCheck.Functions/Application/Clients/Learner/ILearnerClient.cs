using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.Learner
{
    public interface ILearnerClient
    {
        Task<LearnerNiNumber> GetNiNumber(Models.EmploymentCheck check);
    }
}
