using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecks
{
    public class GetEmploymentChecksQueryResult
    {
        public GetEmploymentChecksQueryResult(IList<Application.Models.Domain.EmploymentCheckModel> employmentCheckModels)
        {
            EmploymentCheckModels = employmentCheckModels;
        }

        public IList<Application.Models.Domain.EmploymentCheckModel> EmploymentCheckModels { get; }
    }
}
