namespace SFA.DAS.EmploymentCheck.Api.Commands.RegisterCheckCommand
{
    public class RegisterCheckResult
    {
        public int? VersionId { get; set; }
        public string? ErrorType { get; set; }
        public string? ErrorMessage { get; set; }
    }
}