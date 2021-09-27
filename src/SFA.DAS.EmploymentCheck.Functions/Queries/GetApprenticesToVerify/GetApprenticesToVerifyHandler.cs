using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Queries.GetApprenticesToVerify
{
    public class GetApprenticesToVerifyHandler : IRequestHandler<GetApprenticesToVerifyRequest, GetApprenticesToVerifyResult>
    {
        public async Task<GetApprenticesToVerifyResult> Handle(GetApprenticesToVerifyRequest request, CancellationToken cancellationToken)
        {
            return new GetApprenticesToVerifyResult(new List<ApprenticeToVerifyDto> { new ApprenticeToVerifyDto(123, "ssdjhgffg", 34354, 4356456, 12345, DateTime.Now, DateTime.Now ) });
        }
    }
}
