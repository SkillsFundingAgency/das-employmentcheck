namespace SFA.DAS.EmploymentCheck.Queries.ResetEmploymentChecksMessageSentDate
{
    public class ResetEmploymentChecksMessageSentDateQueryResult
    {
        public ResetEmploymentChecksMessageSentDateQueryResult(long updatedRowsCount)
        {
            UpdatedRowsCount = updatedRowsCount;
        }

        public long UpdatedRowsCount { get; }
    }
}