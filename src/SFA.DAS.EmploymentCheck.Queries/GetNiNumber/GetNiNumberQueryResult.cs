using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Queries.GetNiNumber
{
    public class GetNiNumberQueryResult
    {
        public GetNiNumberQueryResult(LearnerNiNumber learnerNiNumber)
        {
            LearnerNiNumber = learnerNiNumber;
        }

        public LearnerNiNumber LearnerNiNumber { get; }
    }
}
