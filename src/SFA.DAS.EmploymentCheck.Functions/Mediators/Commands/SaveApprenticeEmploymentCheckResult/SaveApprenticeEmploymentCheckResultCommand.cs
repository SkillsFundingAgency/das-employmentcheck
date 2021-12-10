using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SaveApprenticeEmploymentCheckResult
{
    public class SaveApprenticeEmploymentCheckResultCommand : IRequest
    {
        public SaveApprenticeEmploymentCheckResultCommand(
            ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel)
        {
            ApprenticeEmploymentCheckMessageModel = apprenticeEmploymentCheckMessageModel;
        }

        public ApprenticeEmploymentCheckMessageModel ApprenticeEmploymentCheckMessageModel { get; }
    }
}
