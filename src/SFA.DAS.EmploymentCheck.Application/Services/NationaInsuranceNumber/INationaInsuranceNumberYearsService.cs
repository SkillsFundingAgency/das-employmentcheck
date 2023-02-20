using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber
{
    public interface INationalInsuranceNumberYearsService
    {
        Task<IEnumerable<string>> Get();
    }
}
