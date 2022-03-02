using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Services.Learner
{
    public interface ILearnerService
    {
        Task<LearnerNiNumber> GetDbNiNumber(Models.EmploymentCheck employmentCheck);

        Task<LearnerNiNumber> GetNiNumber(Models.EmploymentCheck employmentCheck);
    }
}