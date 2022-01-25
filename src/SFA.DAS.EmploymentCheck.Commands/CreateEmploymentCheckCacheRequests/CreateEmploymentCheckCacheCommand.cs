using MediatR;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequests
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
