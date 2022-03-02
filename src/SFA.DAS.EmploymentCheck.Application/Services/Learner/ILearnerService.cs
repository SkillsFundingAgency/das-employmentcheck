using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Services.Learner
{
    public interface ILearnerService
    {
        Task<LearnerNiNumber> GetDbNiNumber(Data.Models.EmploymentCheck employmentCheck);

        Task<LearnerNiNumber> GetNiNumber(Data.Models.EmploymentCheck employmentCheck);
    }
}