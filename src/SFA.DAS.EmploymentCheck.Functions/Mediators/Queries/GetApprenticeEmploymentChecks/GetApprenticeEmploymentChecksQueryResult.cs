using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticeEmploymentChecks
{
    public class GetApprenticeEmploymentChecksQueryResult
    {
        public GetApprenticeEmploymentChecksQueryResult(IList<EmploymentCheckModel> apprenticeEmploymentChecks)
        {
            ApprenticeEmploymentChecks = apprenticeEmploymentChecks;
        }

        public IList<EmploymentCheckModel> ApprenticeEmploymentChecks { get; }
    }
}
