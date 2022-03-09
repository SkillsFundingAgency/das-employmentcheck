namespace SFA.DAS.EmploymentCheck.Queries.GetNiNumber
{
    public class GetNiNumberQueryRequest : IQuery
    {
        public GetNiNumberQueryRequest(Data.Models.EmploymentCheck check)
        {
            Check = check;
        }

        public Data.Models.EmploymentCheck Check { get; }
    }
}
