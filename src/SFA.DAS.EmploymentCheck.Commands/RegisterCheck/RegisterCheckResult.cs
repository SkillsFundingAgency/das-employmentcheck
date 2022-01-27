﻿namespace SFA.DAS.EmploymentCheck.Commands.RegisterCheck
{
    public class RegisterCheckResult
    {
        public string ErrorType { get; set; }
        public string ErrorMessage { get; set; }

        public bool Invalid() => !string.IsNullOrEmpty(ErrorType) && !string.IsNullOrEmpty(ErrorMessage);
    }
}