using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Gateways
{
    public class HmrcGateway : IHmrcGateway
    {
        public async Task<bool> IsNationalInsuranceNumberRelatedToPayeScheme(string payeScheme, string nationalInsuranceNumber, DateTime startDate)
        {
            return true;
        }
    }
}
