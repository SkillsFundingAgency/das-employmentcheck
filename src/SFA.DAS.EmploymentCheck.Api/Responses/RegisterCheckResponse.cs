using SFA.DAS.EmploymentCheck.Api.Commands;

namespace SFA.DAS.EmploymentCheck.Api.Responses
{
    public class RegisterCheckResponse
    {
        public string? VersionId { get; set; }
        public string? ErrorType { get; set; }
        public string? ErrorMessage { get; set; }
    }
}