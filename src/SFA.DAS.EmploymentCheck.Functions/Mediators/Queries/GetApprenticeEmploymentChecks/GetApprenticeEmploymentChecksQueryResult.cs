using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticeEmploymentChecks
{
    public class GetApprenticeEmploymentChecksQueryResult
    {
        public GetApprenticeEmploymentChecksQueryResult(IList<ApprenticeEmploymentCheckModel> apprenticeEmploymentChecks)
        {
            ApprenticeEmploymentChecks = apprenticeEmploymentChecks;
        }

        public IList<ApprenticeEmploymentCheckModel> ApprenticeEmploymentChecks { get; }
    }
}
