using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.EmploymentCheck.Api.Mediators.Commands.RegisterCheckCommand
{
    public class RegisterCheckCommandValidator : IRegisterCheckCommandValidator
    {
        public RegisterCheckResult Validate(RegisterCheckCommand command)
        {
            var errorTypes = new List<string>();
            var errorMessages = new List<string>();

            if (command.CorrelationId == Guid.Empty || string.IsNullOrEmpty(command.CheckType) || command.Uln == 0 || command.ApprenticeshipAccountId == 0)
            {
                errorTypes.Add("Bad_Data");
                errorMessages.Add("Missing data not supplied");
            }

            if (command.MinDate >= command.MaxDate)
            {
                errorTypes.Add("Bad_DateRange");
                errorMessages.Add("Min date must be before Max date");
            }

            if (errorTypes.Count > 0)
            {
                var responseErrorTypes = new StringBuilder();
                foreach (var errorType in errorTypes)
                {
                    if (responseErrorTypes.Length > 0)
                    {
                        responseErrorTypes.Append(", ");
                    }
                    responseErrorTypes.Append($"{errorType}");
                }

                var responseErrorMessages = new StringBuilder();
                foreach (var errorMessage in errorMessages)
                {
                    if (responseErrorMessages.Length > 0)
                    {
                        responseErrorMessages.Append(", ");
                    }
                    responseErrorMessages.Append($"{errorMessage}");
                }

                return new RegisterCheckResult
                {
                    ErrorMessage = responseErrorMessages.ToString(),
                    ErrorType = responseErrorTypes.ToString()
                };
            }
            
            return new RegisterCheckResult();
        }
    }
}