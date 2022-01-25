using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Queries.GetNiNumbers
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
