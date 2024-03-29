﻿namespace SFA.DAS.EmploymentCheck.Api.Mediators.Commands.RegisterCheckCommand
{
    public class RegisterCheckResult
    {
        public string ErrorType { get; set; }
        public string ErrorMessage { get; set; }

        public bool Invalid() => !string.IsNullOrEmpty(ErrorType) && !string.IsNullOrEmpty(ErrorMessage);
    }
}