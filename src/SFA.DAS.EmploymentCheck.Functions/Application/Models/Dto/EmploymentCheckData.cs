using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class EmploymentCheckData
    {
        public EmploymentCheckData() { }

        public EmploymentCheckData(
            IList<EmploymentCheckModel> employmentChecks,
            IList<ApprenticeNiNumber> apprenticeNiNumbers,
            IList<EmployerPayeSchemes> employerPayeSchemes)
        {
            EmploymentChecks = employmentChecks;
            ApprenticeNiNumbers = apprenticeNiNumbers;
            EmployerPayeSchemes = employerPayeSchemes;
        }

        public IList<EmploymentCheckModel> EmploymentChecks { get; set;  }

        public IList<ApprenticeNiNumber> ApprenticeNiNumbers { get; set; }

        public IList<EmployerPayeSchemes> EmployerPayeSchemes { get; set; }
    }
}
