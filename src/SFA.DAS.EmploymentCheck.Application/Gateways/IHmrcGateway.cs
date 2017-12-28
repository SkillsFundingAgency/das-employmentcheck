using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Gateways
{
    public interface IHmrcGateway
    {
        Task<bool> IsNationalInsuranceNumberRelatedToPayeScheme(string payeScheme, string nationalInsuranceNumber, DateTime startDate);
    }
}
