using SFA.DAS.EmploymentCheck.Functions.Commands.CheckApprentice;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Services
{
    public interface IHmrcService
    {
        Task<bool> IsNationalInsuranceNumberRelatedToPayeScheme(string payeScheme, CheckApprenticeCommand checkApprenticeCommand, DateTime startDate, DateTime endDate);
    }
}
