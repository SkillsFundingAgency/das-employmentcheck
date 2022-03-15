namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public interface IEmploymentCheckDataValidator
    {
        string EmploymentCheckDataHasError(EmploymentCheckData employmentCheckData);
    }
}
