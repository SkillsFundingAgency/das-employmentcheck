using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentCheck
{
    public class GetEmploymentCheckQueryResult
    {
        public GetEmploymentCheckQueryResult() { }

        public GetEmploymentCheckQueryResult(Models.EmploymentCheck employmentCheck)
        {
            EmploymentCheck = employmentCheck;
        }

        public Models.EmploymentCheck EmploymentCheck { get; set; }
    }
}