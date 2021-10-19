using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetLearnersRequiringEmploymentCheck
{
    public class GetLearnersRequiringEmploymentCheckResult
    {
        public GetLearnersRequiringEmploymentCheckResult(List<LearnersRequiringEmploymentCheckDto> learnersRequiringEmploymentCheckDtos)
        {
            LearnersRequiringEmploymentCheckDtos = learnersRequiringEmploymentCheckDtos;
        }

        public List<LearnersRequiringEmploymentCheckDto> LearnersRequiringEmploymentCheckDtos { get; }
    }
}
