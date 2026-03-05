using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Api.Responses;

public class GetEmploymentChecksResponse
{
    public IReadOnlyList<EvsCheckResponse> Checks { get; set; }
}
