using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckEmploymentStatus
{
    public class CheckEmploymentStatusQueryResult
    {
        public CheckEmploymentStatusQueryResult(EmploymentCheckMessage employmentCheckMessage)
        {
            EmploymentCheckMessage = employmentCheckMessage;
        }

        public EmploymentCheckMessage EmploymentCheckMessage { get; }
    }
}
