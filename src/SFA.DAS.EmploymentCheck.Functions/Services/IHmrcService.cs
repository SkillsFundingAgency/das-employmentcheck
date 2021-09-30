using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Services
{
    public interface IHmrcService
    {
        Task<bool> IsNationalInsuranceNumberRelatedToPayeScheme(string payeScheme, string nationalInsuranceNumber, DateTime startDate, DateTime endDate);
    }
}
