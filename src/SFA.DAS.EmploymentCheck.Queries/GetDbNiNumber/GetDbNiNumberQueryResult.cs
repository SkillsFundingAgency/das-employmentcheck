using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Queries.GetDbNiNumber
{
    public class GetDbNiNumberQueryResult
    {
        public GetDbNiNumberQueryResult(LearnerNiNumber learnerNiNumber)
        {
            LearnerNiNumber = learnerNiNumber;
        }

        public LearnerNiNumber LearnerNiNumber { get; }
    }
}
