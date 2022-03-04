using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetDbNiNumbers
{
    public class GetDbNiNumbersQueryResult
    {
        public GetDbNiNumbersQueryResult(IList<LearnerNiNumber> learnerNiNumbers)
        {
            LearnerNiNumbers = learnerNiNumbers;
        }

        public IList<LearnerNiNumber> LearnerNiNumbers { get; }
    }
}
