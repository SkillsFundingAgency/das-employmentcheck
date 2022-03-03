namespace SFA.DAS.EmploymentCheck.Queries.GetEmploymentCheck
{
    public class GetEmploymentCheckQueryResult
    {
        public GetEmploymentCheckQueryResult(Data.Models.EmploymentCheck employmentCheck)
        {
            EmploymentCheck = employmentCheck;
        }

        public Data.Models.EmploymentCheck EmploymentCheck { get; }
    }
}