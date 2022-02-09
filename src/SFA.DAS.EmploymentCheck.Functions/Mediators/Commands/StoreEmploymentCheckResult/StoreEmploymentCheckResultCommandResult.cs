namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.StoreEmploymentCheckResult
{
    public class StoreEmploymentCheckResultCommandResult
    {
        public StoreEmploymentCheckResultCommandResult(
            long employmentCheckId
        )
        {
            EmploymentCheckId = employmentCheckId;
        }

        public long EmploymentCheckId { get; }
    }
}

