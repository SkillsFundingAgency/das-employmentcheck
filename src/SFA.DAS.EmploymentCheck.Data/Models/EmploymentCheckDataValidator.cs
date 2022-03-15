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

        public string EmploymentCheckDataHasError(EmploymentCheckData employmentCheckData)
        {
            var ninoHasError = _learnerNiValidator.NinoHasError(employmentCheckData);
            var payeSchemesHasError = _employerPayeSchemesValidator.PayeSchemesHasError(employmentCheckData);

            if(ninoHasError != null && payeSchemesHasError == null)
            {
                return ninoHasError;
            }

            if (ninoHasError == null && payeSchemesHasError != null)
            {
                return payeSchemesHasError;
            }

            if (ninoHasError != null)
            {
                return $"{ninoHasError}And{payeSchemesHasError}";
            }

            return null;
        }
    }
}
