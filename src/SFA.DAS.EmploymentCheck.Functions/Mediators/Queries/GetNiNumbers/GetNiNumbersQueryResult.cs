using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetNiNumbers
{
    public class GetNiNumbersQueryResult
    {
        public GetNiNumbersQueryResult(IList<LearnerNiNumber> learnerNiNumber)
        {
            LearnerNiNumber = learnerNiNumber;
        }

        public IList<LearnerNiNumber> LearnerNiNumber { get; }
    }
}
