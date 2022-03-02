namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    public class EmploymentCheckData
    {
        public EmploymentCheckData(
            EmploymentCheck employmentCheck,
            LearnerNiNumber apprenticeNiNumber,
            EmployerPayeSchemes employerPayeSchemes)
        {
            EmploymentCheck = employmentCheck;
            ApprenticeNiNumber = apprenticeNiNumber;
            EmployerPayeSchemes = employerPayeSchemes;
        }

        public EmploymentCheck EmploymentCheck { get; set;  }

        public LearnerNiNumber ApprenticeNiNumber { get; set; }

        public EmployerPayeSchemes EmployerPayeSchemes { get; set; }
    }
}
