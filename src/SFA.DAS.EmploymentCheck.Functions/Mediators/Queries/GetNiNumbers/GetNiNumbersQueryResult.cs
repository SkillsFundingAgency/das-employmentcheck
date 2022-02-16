using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetNiNumbers
{
    public class GetNiNumbersQueryResult
    {
        public GetNiNumbersQueryResult(LearnerNiNumber learnerNiNumber)
        {
            LearnerNiNumber = learnerNiNumber;
        }

        public LearnerNiNumber LearnerNiNumber { get; }
    }
}
