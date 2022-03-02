using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Clients.Learner
{
    public interface ILearnerClient
    {
        Task<LearnerNiNumber> GetDbNiNumber(Data.Models.EmploymentCheck check);

        Task<LearnerNiNumber> GetNiNumber(Data.Models.EmploymentCheck check);
    }
}
