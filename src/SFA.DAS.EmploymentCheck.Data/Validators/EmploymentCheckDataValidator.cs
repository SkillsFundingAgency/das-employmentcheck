using FluentValidation;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System;

namespace SFA.DAS.EmploymentCheck.Data.Validators
{
    [Obsolete("Use command validators")]
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
