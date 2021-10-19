using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice
{
    public class CheckApprenticeCommand : IRequest
    {
        public CheckApprenticeCommand(ApprenticeToVerifyDto apprentice)
        {
            Apprentice = apprentice;
        }

        public ApprenticeToVerifyDto Apprentice { get; }
    }
}
