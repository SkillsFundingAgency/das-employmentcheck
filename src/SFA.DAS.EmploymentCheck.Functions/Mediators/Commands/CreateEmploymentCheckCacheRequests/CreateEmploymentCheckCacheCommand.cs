using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests
{
    public class CreateEmploymentCheckCacheCommand : IRequest
    {
        public CreateEmploymentCheckCacheCommand(EmploymentCheckData employmentCheckData)
        {
            EmploymentCheckData = employmentCheckData;
        }

        public EmploymentCheckData EmploymentCheckData { get; }
    }
}
