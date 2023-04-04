using SFA.DAS.EmploymentCheck.Data.Models;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber
{
    public class NationalInsuranceNumberServiceWithAYLookup : INationalInsuranceNumberService
    {
        private readonly INationalInsuranceNumberService _nationalInsuranceNumberService;
        private readonly INationalInsuranceNumberYearsService _nationalInsuranceNumberServiceYears;

        public NationalInsuranceNumberServiceWithAYLookup(
            INationalInsuranceNumberService nationalInsuranceNumberService,
            INationalInsuranceNumberYearsService nationalInsuranceNumberServiceYears)
        {
            _nationalInsuranceNumberService = nationalInsuranceNumberService;
            _nationalInsuranceNumberServiceYears = nationalInsuranceNumberServiceYears;
        }

        public async Task<LearnerNiNumber> Get(NationalInsuranceNumberRequest nationalInsuranceNumberRequest)
        {
            LearnerNiNumber response = null;

            foreach (var academicYear in await _nationalInsuranceNumberServiceYears.Get())
            {
                var request = new NationalInsuranceNumberRequest(nationalInsuranceNumberRequest.EmploymentCheck, academicYear);
                response = await _nationalInsuranceNumberService.Get(request);
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    return response;
                }
            }
            return response;
        }
    }
}
