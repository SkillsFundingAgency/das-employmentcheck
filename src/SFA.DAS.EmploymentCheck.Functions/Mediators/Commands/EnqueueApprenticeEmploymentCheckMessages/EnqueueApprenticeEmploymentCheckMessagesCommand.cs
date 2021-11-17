using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice
{
    public class EnqueueApprenticeEmploymentCheckMessagesCommand
        : IRequest
    {
        public EnqueueApprenticeEmploymentCheckMessagesCommand(
            ApprenticeRelatedData apprenticeRelatedData)
        {
            ApprenticeRelatedData = apprenticeRelatedData;
        }

        public ApprenticeRelatedData ApprenticeRelatedData { get; }
    }
}
