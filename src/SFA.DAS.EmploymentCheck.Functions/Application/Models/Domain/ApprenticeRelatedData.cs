using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class ApprenticeRelatedData
    {
        public ApprenticeRelatedData() { }

        public ApprenticeRelatedData(
            IList<ApprenticeEmploymentCheckModel> apprenticeEmploymentChecks,
            IList<ApprenticeNiNumber> apprenticeNiNumbers,
            IList<EmployerPayeSchemes> employerPayeSchemes)
        {
            ApprenticeEmploymentChecks = apprenticeEmploymentChecks;
            ApprenticeNiNumbers = apprenticeNiNumbers;
            EmployerPayeSchemes = employerPayeSchemes;
        }

        public IList<ApprenticeEmploymentCheckModel> ApprenticeEmploymentChecks { get; set;  }

        public IList<ApprenticeNiNumber> ApprenticeNiNumbers { get; set; }

        public IList<EmployerPayeSchemes> EmployerPayeSchemes { get; set; }
    }
}
