namespace SFA.DAS.EmploymentCheck.Domain.Enums
{
    public enum ProcessingCompletionStatus
    {
        Started = 10,
        ProcessingError_NinoNotFound = 101,
        ProcessingError_PayeSchemeNotFound = 102,
        Completed = 200
    }
}
