using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
{
    public interface IHmrcService
    {
        Task<ApprenticeEmploymentCheckMessageModel> IsNationalInsuranceNumberRelatedToPayeScheme(ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel);
    }
}
