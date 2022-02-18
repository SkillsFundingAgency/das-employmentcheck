using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Learner
{
    public interface ILearnerService
    {
        Task<LearnerNiNumber> GetNiNumber(Models.EmploymentCheck check);
    }
}
