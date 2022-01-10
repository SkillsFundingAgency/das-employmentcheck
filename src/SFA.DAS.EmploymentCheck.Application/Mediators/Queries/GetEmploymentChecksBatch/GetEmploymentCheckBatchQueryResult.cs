using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetEmploymentChecksBatch
{
    public class GetEmploymentCheckBatchQueryResult
    {
        public GetEmploymentCheckBatchQueryResult(IList<Domain.Entities.EmploymentCheck> employmentChecks)
        {
            EmploymentChecks = employmentChecks;
        }

        public IList<Domain.Entities.EmploymentCheck> EmploymentChecks { get; }
    }
}
