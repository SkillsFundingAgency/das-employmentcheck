using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecksBatch
{
    public class GetEmploymentCheckBatchQueryResult
    {
        public GetEmploymentCheckBatchQueryResult() { }

        public GetEmploymentCheckBatchQueryResult(IList<Application.Models.EmploymentCheck> apprenticeEmploymentChecks)
        {
            ApprenticeEmploymentChecks = apprenticeEmploymentChecks;
        }

        public IList<Application.Models.EmploymentCheck> ApprenticeEmploymentChecks { get; set; }
    }
}