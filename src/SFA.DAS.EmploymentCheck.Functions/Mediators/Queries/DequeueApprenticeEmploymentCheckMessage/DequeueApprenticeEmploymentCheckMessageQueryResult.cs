using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.DequeueApprenticeEmploymentCheckMessage
{
    public class DequeueApprenticeEmploymentCheckMessageQueryResult
    {
        public DequeueApprenticeEmploymentCheckMessageQueryResult(
            ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessage)
        {
            ApprenticeEmploymentCheckMessage = apprenticeEmploymentCheckMessage;
        }

        public ApprenticeEmploymentCheckMessageModel ApprenticeEmploymentCheckMessage { get; }
    }
}
