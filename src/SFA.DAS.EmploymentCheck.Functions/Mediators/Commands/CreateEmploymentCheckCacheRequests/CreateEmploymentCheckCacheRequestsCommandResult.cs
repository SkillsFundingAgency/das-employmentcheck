using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests
{
    public class CreateEmploymentCheckCacheRequestsCommandResult
    {
        public CreateEmploymentCheckCacheRequestsCommandResult(IList<EmploymentCheckCacheRequest> employmentCheckCacheRequests)
        {
            EmploymentCheckCacheRequests = employmentCheckCacheRequests;
        }

        public IList<EmploymentCheckCacheRequest> EmploymentCheckCacheRequests { get; }
    }
}
