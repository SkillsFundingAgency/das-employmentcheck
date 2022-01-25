using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Queries.GetEmploymentChecksBatch
{
    public class GetEmploymentCheckBatchQueryResult
    {
        public GetEmploymentCheckBatchQueryResult(IList<Data.Models.EmploymentCheck> apprenticeEmploymentChecks)
        {
            ApprenticeEmploymentChecks = apprenticeEmploymentChecks;
        }

        public IList<Data.Models.EmploymentCheck> ApprenticeEmploymentChecks { get; }
    }
}
