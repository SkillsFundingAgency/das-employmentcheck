using FluentValidation;
using SFA.DAS.EmploymentCheck.Domain.Common.Dtos;

namespace SFA.DAS.EmploymentCheck.Application.Common.Behaviours.Validators
{
    public class EmploymentCheckDataValidator
        : AbstractValidator<EmploymentCheckData>
    {
        public EmploymentCheckDataValidator()
        {
            RuleFor(employmentCheckData => employmentCheckData.EmploymentChecks).NotEmpty();
            RuleFor(employmentCheckData => employmentCheckData.ApprenticeNiNumbers).NotEmpty();
            RuleFor(employmentCheckData => employmentCheckData.EmployerPayeSchemes).NotEmpty();
        }
    }
}
