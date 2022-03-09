namespace SFA.DAS.EmploymentCheck.Queries.GetDbNiNumber
{
    public class GetDbNiNumberQueryRequest: IQuery
    {
        public GetDbNiNumberQueryRequest(Data.Models.EmploymentCheck employmentCheck)
        {
            EmploymentCheck = employmentCheck;
        }

        public Data.Models.EmploymentCheck EmploymentCheck { get; }
    }
}
