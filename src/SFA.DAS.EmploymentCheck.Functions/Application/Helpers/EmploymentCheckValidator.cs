using FluentValidation;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Helpers
{
    public class EmploymentCheckValidator
        : AbstractValidator<Models.EmploymentCheck>
    {
        public EmploymentCheckValidator()
        {
            RuleFor(apprenticeEmploymentCheck => apprenticeEmploymentCheck.Id).NotEmpty();
            RuleFor(apprenticeEmploymentCheck => apprenticeEmploymentCheck.CheckType).NotEmpty();
            RuleFor(apprenticeEmploymentCheck => apprenticeEmploymentCheck.Uln).NotEmpty();
            RuleFor(apprenticeEmploymentCheck => apprenticeEmploymentCheck.ApprenticeshipId).NotEmpty();
            RuleFor(apprenticeEmploymentCheck => apprenticeEmploymentCheck.AccountId).NotEmpty();
            RuleFor(apprenticeEmploymentCheck => apprenticeEmploymentCheck.MinDate).NotEqual(System.DateTime.MinValue);
            RuleFor(apprenticeEmploymentCheck => apprenticeEmploymentCheck.MaxDate).NotEqual(System.DateTime.MinValue);
        }
    }
}
