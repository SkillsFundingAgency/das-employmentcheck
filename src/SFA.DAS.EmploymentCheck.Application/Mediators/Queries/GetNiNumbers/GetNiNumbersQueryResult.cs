using SFA.DAS.EmploymentCheck.Application.Common.Models;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetNiNumbers
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
