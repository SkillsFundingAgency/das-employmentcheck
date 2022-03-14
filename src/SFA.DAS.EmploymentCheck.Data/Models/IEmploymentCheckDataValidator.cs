namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public interface IEmploymentCheckDataValidator
    {
        (bool IsValid, string ErrorType) IsValidEmploymentCheckData(EmploymentCheckData employmentCheckData);
    }
}
