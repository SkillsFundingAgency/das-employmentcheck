using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SaveApprenticeEmploymentCheckResult
{
    public class SaveApprenticeEmploymentCheckResultCommand : IRequest
    {
        public SaveApprenticeEmploymentCheckResultCommand(
            EmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel)
        {
            ApprenticeEmploymentCheckMessageModel = apprenticeEmploymentCheckMessageModel;
        }

        public EmploymentCheckMessageModel ApprenticeEmploymentCheckMessageModel { get; }
    }
}
