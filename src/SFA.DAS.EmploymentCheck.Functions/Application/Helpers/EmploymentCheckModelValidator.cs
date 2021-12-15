using FluentValidation;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Helpers
{
    public class EmploymentCheckModelValidator
        : AbstractValidator<EmploymentCheckModel>
    {
        public EmploymentCheckModelValidator()
        {
            RuleFor(employmentCheckModel => employmentCheckModel.Id).NotEmpty();
            RuleFor(employmentCheckModel => employmentCheckModel.CheckType).NotEmpty();
            RuleFor(employmentCheckModel => employmentCheckModel.Uln).NotEmpty();
            RuleFor(employmentCheckModel => employmentCheckModel.ApprenticeshipId).NotEmpty();
            RuleFor(employmentCheckModel => employmentCheckModel.AccountId).NotEmpty();
            RuleFor(employmentCheckModel => employmentCheckModel.MinDate).NotEqual(System.DateTime.MinValue);
            RuleFor(employmentCheckModel => employmentCheckModel.MaxDate).NotEqual(System.DateTime.MinValue);
        }
    }
}
