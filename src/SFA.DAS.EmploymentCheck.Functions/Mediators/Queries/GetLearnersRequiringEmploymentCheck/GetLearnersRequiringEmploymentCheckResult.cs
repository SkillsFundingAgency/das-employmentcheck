using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetLearnersRequiringEmploymentCheck
{
    public class GetLearnersRequiringEmploymentCheckResult
    {
        public GetLearnersRequiringEmploymentCheckResult(List<LearnerRequiringEmploymentCheckDto> learnersRequiringEmploymentCheckDtos)
        {
            LearnersRequiringEmploymentCheckDtos = learnersRequiringEmploymentCheckDtos;
        }

        public List<LearnerRequiringEmploymentCheckDto> LearnersRequiringEmploymentCheckDtos { get; }
    }
}
