namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public interface IEmployerPayeSchemesValidator
    {
        string PayeSchemesHasError(EmploymentCheckData employmentCheckData);
    }
}
