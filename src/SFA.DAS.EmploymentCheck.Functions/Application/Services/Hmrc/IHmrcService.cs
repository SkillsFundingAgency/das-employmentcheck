using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
{
    public interface IHmrcService
    {
        Task<EmploymentCheckMessage> IsNationalInsuranceNumberRelatedToPayeScheme(EmploymentCheckMessage request);
    }
}
