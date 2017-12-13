using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Domain.Models
{
    public class PreviouslyHandledSubmissionEventViewModel
    {
        public IEnumerable<PreviousHandledSubmissionEvent> Events { get; set; }
    }
}