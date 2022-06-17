using System;

namespace SFA.DAS.EmploymentCheck.Types
{
    public class EmploymentCheckCompletedEvent
    {
        public Guid CorrelationId { get; }
        public bool? EmploymentResult { get; }
        public DateTime CheckDate { get; }
        public string ErrorType { get; }

        public EmploymentCheckCompletedEvent(Guid correlationId, bool? employmentResult, DateTime checkDate, string errorType)
        {
            CorrelationId = correlationId;
            EmploymentResult = employmentResult;
            CheckDate = checkDate;
            ErrorType = errorType;
        }
    }
}
