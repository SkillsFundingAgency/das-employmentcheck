using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.DequeueApprenticeEmploymentCheckMessage
{
    public class DequeueApprenticeEmploymentCheckMessageQueryResult
    {
        public DequeueApprenticeEmploymentCheckMessageQueryResult(
            EmploymentCheckMessage apprenticeEmploymentCheckMessage)
        {
            ApprenticeEmploymentCheckMessage = apprenticeEmploymentCheckMessage;
        }

        public EmploymentCheckMessage ApprenticeEmploymentCheckMessage { get; }
    }
}
