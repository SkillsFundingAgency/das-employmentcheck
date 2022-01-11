using SFA.DAS.EmploymentCheck.Application.Common.Models;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Domain.Common.Dtos

{
    public class EmploymentCheckData
    {
        public EmploymentCheckData() { }

        public EmploymentCheckData(
            IList<Domain.Entities.EmploymentCheck> employmentChecks,
            IList<LearnerNiNumber> apprenticeNiNumbers,
            IList<EmployerPayeSchemes> employerPayeSchemes)
        {
            EmploymentChecks = employmentChecks;
            ApprenticeNiNumbers = apprenticeNiNumbers;
            EmployerPayeSchemes = employerPayeSchemes;
        }

        public IList<Domain.Entities.EmploymentCheck> EmploymentChecks { get; set;  }

        public IList<LearnerNiNumber> ApprenticeNiNumbers { get; set; }

        public IList<EmployerPayeSchemes> EmployerPayeSchemes { get; set; }
    }
}
