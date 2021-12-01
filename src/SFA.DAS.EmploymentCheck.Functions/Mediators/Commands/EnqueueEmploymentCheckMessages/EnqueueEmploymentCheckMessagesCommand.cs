using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.EnqueueEmploymentCheckMessages
{
    public class EnqueueEmploymentCheckMessagesCommand
        : IRequest
    {
        public EnqueueEmploymentCheckMessagesCommand(
            EmploymentCheckData apprenticeRelatedData)
        {
            ApprenticeRelatedData = apprenticeRelatedData;
        }

        public EmploymentCheckData ApprenticeRelatedData { get; }
    }
}
