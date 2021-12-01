using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SaveEmploymentCheckResult
{
    public class SaveEmploymentCheckResultCommand : IRequest
    {
        public SaveEmploymentCheckResultCommand(
            EmploymentCheckMessage employmentCheckMessage)
        {
            EmploymentCheckMessage = employmentCheckMessage;
        }

        public EmploymentCheckMessage EmploymentCheckMessage { get; }
    }
}
