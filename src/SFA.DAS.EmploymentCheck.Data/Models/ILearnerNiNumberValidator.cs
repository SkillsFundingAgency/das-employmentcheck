namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public interface ILearnerNiNumberValidator
    {
        string NinoHasError(EmploymentCheckData employmentCheckData);
    }
}
