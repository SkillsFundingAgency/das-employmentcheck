using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetNiNumber
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
