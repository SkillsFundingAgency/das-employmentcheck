namespace SFA.DAS.EmploymentCheck.Queries.GetResponseEmploymentCheck
{
    public class GetResponseEmploymentCheckQueryResult
    {
        public GetResponseEmploymentCheckQueryResult(Data.Models.EmploymentCheck employmentCheck)
        {
            EmploymentCheck = employmentCheck;
        }

        public Data.Models.EmploymentCheck EmploymentCheck { get; }
    }
}