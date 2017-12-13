namespace SFA.DAS.EmploymentCheck.Domain.Models
{
    public class PreviousHandledSubmissionEvent
    {
        public string NiNumber { get; set; }
        public string Uln { get; set; }
        public bool PassedValidationCheck { get; set; }
    }
}
