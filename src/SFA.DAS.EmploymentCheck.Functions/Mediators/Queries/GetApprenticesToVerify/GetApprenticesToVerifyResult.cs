using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesToVerify
{
    public class GetApprenticesToVerifyResult
    {
        public GetApprenticesToVerifyResult(List<ApprenticeToVerifyDto> apprenticesToVerify)
        {
            ApprenticesToVerify = apprenticesToVerify;
        }

        public List<ApprenticeToVerifyDto> ApprenticesToVerify { get; }
    }
}
