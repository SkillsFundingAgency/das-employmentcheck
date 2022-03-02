using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequest
{
    public class CreateEmploymentCheckCacheRequestCommand : IRequest
    {
        public CreateEmploymentCheckCacheRequestCommand(EmploymentCheckData employmentCheckData)
        {
            EmploymentCheckData = employmentCheckData;
        }

        public EmploymentCheckData EmploymentCheckData { get; }
    }
}
