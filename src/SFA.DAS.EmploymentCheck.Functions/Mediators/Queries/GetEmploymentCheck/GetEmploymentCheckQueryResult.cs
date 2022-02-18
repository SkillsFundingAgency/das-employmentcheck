using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentCheck
{
    public class GetEmploymentCheckQueryResult
    {
        public GetEmploymentCheckQueryResult(IList<Application.Models.EmploymentCheck> apprenticeEmploymentChecks)
        {
            ApprenticeEmploymentChecks = apprenticeEmploymentChecks;
        }

        public IList<Application.Models.EmploymentCheck> ApprenticeEmploymentChecks { get; }
    }
}
