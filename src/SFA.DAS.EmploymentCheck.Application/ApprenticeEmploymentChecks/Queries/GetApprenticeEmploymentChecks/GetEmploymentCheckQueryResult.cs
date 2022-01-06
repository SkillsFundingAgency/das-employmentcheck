using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Application.ApprenticeEmploymentChecks.Queries.GetApprenticeEmploymentChecks.Models
{
    public class GetEmploymentCheckQueryResult
    {
        public GetEmploymentCheckQueryResult(IList<ApprenticeEmploymentCheckDto> apprenticeEmploymentChecks)
        {
            ApprenticeEmploymentChecks = apprenticeEmploymentChecks;
        }

        public IList<ApprenticeEmploymentCheckDto> ApprenticeEmploymentChecks { get; }
    }
}
