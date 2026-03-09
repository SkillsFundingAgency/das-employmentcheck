namespace SFA.DAS.EmploymentCheck.Api.Responses;

public class EvsCheckResult
{
    public bool? Employed { get; set; }
    public int? CompletionStatus { get; set; }
    public string ErrorCode { get; set; }
}
