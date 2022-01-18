using System;
using System.Collections.Generic;

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
                var responseErrorTypes = "";
                foreach (var errorType in errorTypes)
                {
                    responseErrorTypes += $"{errorType}, ";
                }
                responseErrorTypes = responseErrorTypes.Trim();
                responseErrorTypes = responseErrorTypes.Trim(',');

                var responseErrorMessages = "";
                foreach (var errorMessage in errorMessages)
                {
                    responseErrorMessages += $"{errorMessage}, ";
                }
                responseErrorMessages = responseErrorMessages.Trim();
                responseErrorMessages = responseErrorMessages.Trim(',');

                return new RegisterCheckResult
                {
                    ErrorMessage = responseErrorMessages,
                    ErrorType = responseErrorTypes
                };
            }
            
            return new RegisterCheckResult();
        }
    }
}