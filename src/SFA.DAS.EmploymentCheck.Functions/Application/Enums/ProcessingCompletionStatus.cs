namespace SFA.DAS.EmploymentCheck.Functions.Application.Enums
{
    public enum ProcessingCompletionStatus
    {
        CompletedSuccessfully = 0,
        HasBeenChecked = 1,                 // used for backward compatibility with the old 'HasBeenChecked' column
        Processing = 10,
        CompletedWithErrors = 20,           // Probably not needed as we are building a list of the error codes below
        Failed_NinoNotFound = 21,
        Failed_PayeSchemeNotFound = 22,
    }
}
