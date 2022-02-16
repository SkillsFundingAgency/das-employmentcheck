using FluentValidation;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Helpers
{
    public class EmploymentCheckDataValidator
        : AbstractValidator<EmploymentCheckData>
    {
        public EmploymentCheckDataValidator()
        {
            RuleFor(employmentCheckData => employmentCheckData.EmploymentCheck).NotEmpty();
            RuleFor(employmentCheckData => employmentCheckData.ApprenticeNiNumber).NotEmpty();
            RuleFor(employmentCheckData => employmentCheckData.EmployerPayeSchemes).NotEmpty();
        }
    }
}
