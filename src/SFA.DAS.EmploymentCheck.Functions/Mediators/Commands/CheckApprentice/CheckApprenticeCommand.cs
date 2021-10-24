using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice
{
    public class CheckApprenticeCommand : IRequest
    {
        public CheckApprenticeCommand(Apprentice apprentice)
        {
            Apprentice = apprentice;
        }

        public Apprentice Apprentice { get; }
    }
}
