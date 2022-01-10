using MediatR;
using SFA.DAS.EmploymentCheck.Domain.Common.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests
{
    public class CreateEmploymentCheckCacheRequestCommand
        : IRequest
    {
        public CreateEmploymentCheckCacheRequestCommand(
            EmploymentCheckData employmentCheckData)
        {
            EmploymentCheckData = employmentCheckData;
        }

        public EmploymentCheckData EmploymentCheckData { get; }
    }
}
