namespace SFA.DAS.EmploymentCheck.Domain.Enums
{
    public enum ProcessingCompletionStatus
    {
        CompletedSuccessfully = 0,
        Processing = 10,
        CompletedWithErrors = 20,
        Failed_NinoNotFound = 21,
        Failed_PayeSchemeNotFound = 22,
    }
}
