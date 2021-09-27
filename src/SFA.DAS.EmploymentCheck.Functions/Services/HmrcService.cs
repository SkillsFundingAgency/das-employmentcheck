using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Services
{
    public class HmrcService : IHmrcService
    {
        public async Task<bool> IsNationalInsuranceNumberRelatedToPayeScheme(string payeScheme, string nationalInsuranceNumber, DateTime startDate, DateTime endDate)
        {
            return false;
        }
    }
}
