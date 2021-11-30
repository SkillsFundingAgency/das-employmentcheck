using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SaveApprenticeEmploymentCheckResult
{
    public class SaveApprenticeEmploymentCheckResultCommand : IRequest
    {
        public SaveApprenticeEmploymentCheckResultCommand(
            EmploymentCheckMessage apprenticeEmploymentCheckMessageModel)
        {
            ApprenticeEmploymentCheckMessageModel = apprenticeEmploymentCheckMessageModel;
        }

        public EmploymentCheckMessage ApprenticeEmploymentCheckMessageModel { get; }
    }
}
