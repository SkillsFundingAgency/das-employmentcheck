using FluentValidation;

namespace SFA.DAS.EmploymentCheck.Application.Common.Behaviours.Validators
{
    public class ApprenticeEmploymentCheckValidator
        : AbstractValidator<Domain.Entities.EmploymentCheck>
    {
        public ApprenticeEmploymentCheckValidator()
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
