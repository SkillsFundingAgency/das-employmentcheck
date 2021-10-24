using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class ApprenticeEmploymentParams
    {
        // TODO: This is just used for a quick test,  delete after use
        public IList<Apprentice> Apprentices { get; set;  }

        public IList<ApprenticeNiNumber> ApprenticeNiNumbers { get; set; }

        public IList<EmployerPayeSchemes> EmployerPayeSchemes { get; set; }
    }
}
