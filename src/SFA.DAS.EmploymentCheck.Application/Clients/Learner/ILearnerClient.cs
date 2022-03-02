using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Application.Clients.Learner
{
    public interface ILearnerClient
    {
        Task<LearnerNiNumber> GetDbNiNumber(Models.EmploymentCheck check);

        Task<LearnerNiNumber> GetNiNumber(Models.EmploymentCheck check);
    }
}
