using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    public class EmploymentCheckData
        : IEmploymentCheckData
    {
        public EmploymentCheckData() { }

        public EmploymentCheckData(
            IList<EmploymentCheck> employmentChecks,
            IList<LearnerNiNumber> apprenticeNiNumbers,
            IList<EmployerPayeSchemes> employerPayeSchemes)
        {
            EmploymentChecks = employmentChecks;
            ApprenticeNiNumbers = apprenticeNiNumbers;
            EmployerPayeSchemes = employerPayeSchemes;
        }

        public IList<EmploymentCheck> EmploymentChecks { get; set;  }

        public IList<LearnerNiNumber> ApprenticeNiNumbers { get; set; }

        public IList<EmployerPayeSchemes> EmployerPayeSchemes { get; set; }
    }
}
