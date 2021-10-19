using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Services
{
    public interface ILearnersService
    {
        Task<LearnerNationalnsuranceNumberDto[]> GetLearnersNationalInsuranceNumbers(LearnerNationalnsuranceNumberDto[] learnersNinosDto);
    }
}
