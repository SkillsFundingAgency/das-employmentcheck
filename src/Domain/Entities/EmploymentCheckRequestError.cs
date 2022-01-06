using System;

namespace SFA.DAS.EmploymentCheck.Domain.Entities
{
    public class EmploymentCheckRequestError
    {
        public long Id { get; set; }

        public long ApprenticeEmploymentCheckId { get; set; }

        public string Source { get; set; }

        public string Message { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime LastUpdatedOn { get; set; }
    }
}
