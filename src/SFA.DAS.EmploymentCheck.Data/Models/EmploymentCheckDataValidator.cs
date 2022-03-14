namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public class EmploymentCheckDataValidator : IEmploymentCheckDataValidator
    {
        private readonly ILearnerNiNumberValidator _learnerNiValidator;
        private readonly IEmployerPayeSchemesValidator _employerPayeSchemesValidator;

        public EmploymentCheckDataValidator(
            ILearnerNiNumberValidator learnerNiValidator,
            IEmployerPayeSchemesValidator employerPayeSchemesValidator
        )
        {
            _learnerNiValidator = learnerNiValidator;
            _employerPayeSchemesValidator = employerPayeSchemesValidator;
        }

        public (bool IsValid, string ErrorType) IsValidEmploymentCheckData(EmploymentCheckData employmentCheckData)
        {
            var ninoStatus = _learnerNiValidator.IsValidNino(employmentCheckData);
            var payeSchemeStatus = _employerPayeSchemesValidator.IsValidPayeScheme(employmentCheckData);

            if(!ninoStatus.IsValid && payeSchemeStatus.IsValid)
            {
                return (false, ninoStatus.ErrorType);
            }
            else if (ninoStatus.IsValid && !payeSchemeStatus.IsValid)
            {
                return (false, payeSchemeStatus.ErrorType);
            }
            else if (!ninoStatus.IsValid && !payeSchemeStatus.IsValid)
            {
                return (false, $"{ninoStatus.ErrorType}And{payeSchemeStatus.ErrorType}");
            }

            return (true, null);
        }
    }
}
