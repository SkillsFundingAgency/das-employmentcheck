using System;

namespace SFA.DAS.EmploymentCheck.Application.ApprenticeEmploymentChecks.Queries.GetApprenticeEmploymentChecks.Models
{
    public class PayeSchemeApiResponseDto
    {
        public Guid? CorrelationId { get; set; }

        public long AccountId { get; set; }

        public string Response { get; set; }
    }
}
