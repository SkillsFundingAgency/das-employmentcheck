namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public interface IEmployerPayeSchemesValidator
    {
        (bool IsValid, string ErrorType) IsValidPayeScheme(EmploymentCheckData employmentCheckData);
    }
}
