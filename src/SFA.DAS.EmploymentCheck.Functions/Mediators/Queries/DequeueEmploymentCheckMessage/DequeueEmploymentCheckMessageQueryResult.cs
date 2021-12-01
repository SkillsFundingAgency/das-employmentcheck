using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.DequeueEmploymentCheckMessage
{
    public class DequeueEmploymentCheckMessageQueryResult
    {
        public DequeueEmploymentCheckMessageQueryResult(
            EmploymentCheckMessage employmentCheckMessage)
        {
            EmploymentCheckMessage = employmentCheckMessage;
        }

        public EmploymentCheckMessage EmploymentCheckMessage { get; }
    }
}
