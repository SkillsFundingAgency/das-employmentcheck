namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public interface ILearnerNiNumberValidator
    {
        (bool IsValid, string ErrorType) IsValidNino(EmploymentCheckData employmentCheckData);
    }
}
