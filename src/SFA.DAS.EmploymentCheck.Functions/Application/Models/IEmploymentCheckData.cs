using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    public interface IEmploymentCheckData
    {
        public IList<EmploymentCheck> EmploymentChecks { get; set;  }

        public IList<LearnerNiNumber> ApprenticeNiNumbers { get; set; }

        public IList<EmployerPayeSchemes> EmployerPayeSchemes { get; set; }
    }
}
