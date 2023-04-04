using SFA.DAS.EmploymentCheck.Data.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber
{
    public interface INationalInsuranceNumberService
    {
        Task<LearnerNiNumber> Get(NationalInsuranceNumberRequest nationalInsuranceNumberRequest);
    }
}
