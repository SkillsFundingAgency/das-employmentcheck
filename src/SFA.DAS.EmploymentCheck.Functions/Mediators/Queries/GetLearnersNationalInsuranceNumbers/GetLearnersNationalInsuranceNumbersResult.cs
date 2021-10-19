using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetLearnersNationalInsuranceNumbers
{
    public class GetLearnersNationalInsuranceNumbersResult
    {
        public GetLearnersNationalInsuranceNumbersResult(List<LearnerNationalnsuranceNumberDto> learnerNationalnsuranceNumberDtos)
        {
            LearnerNationalnsuranceNumberDtos = learnerNationalnsuranceNumberDtos;
        }

        public List<LearnerNationalnsuranceNumberDto> LearnerNationalnsuranceNumberDtos { get; }
    }
}
