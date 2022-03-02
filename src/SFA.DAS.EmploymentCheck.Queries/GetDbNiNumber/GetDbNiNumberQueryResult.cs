using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetDbNiNumber
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
