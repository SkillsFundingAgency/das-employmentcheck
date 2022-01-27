using System;
using FluentValidation;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Data.Validators
{
    [Obsolete("Use command validators")]
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
