﻿using FluentValidation;
using System;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Data.Validators
{
    [Obsolete("Use command validators")]
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
