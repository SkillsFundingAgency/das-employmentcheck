using System.Linq;
using System.Net;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public class EmploymentCheckDataValidator : IEmploymentCheckDataValidator
    {
        public (bool IsValid, string ErrorType) IsValidEmploymentCheckData(EmploymentCheckData employmentCheckData)
        {
            var isValid = true;
            var errorType = string.Empty;

            var ninoStatus = IsValidNino(employmentCheckData);
            var payeSchemeStatus = IsValidPayeScheme(employmentCheckData);

            if(!ninoStatus.IsValid && payeSchemeStatus.IsValid)
            {
                isValid = false;
                errorType = ninoStatus.ErrorType;
            }
            else if (ninoStatus.IsValid && !payeSchemeStatus.IsValid)
            {
                isValid = false;
                errorType = payeSchemeStatus.ErrorType;
            }
            else if (!ninoStatus.IsValid && !payeSchemeStatus.IsValid)
            {
                isValid = false;
                errorType = ninoStatus.ErrorType + "And" + payeSchemeStatus.ErrorType;
            }

            return (isValid, errorType);
        }

        public (bool IsValid, string ErrorType) IsValidNino(EmploymentCheckData employmentCheckData)
        {
            const string NinoNotFound = "NinoNotFound";
            const string NinoInvalid = "NinoInvalid";
            const string NinoFailure = "NinoFailure";
            const int validNinoLength = 9;

            var learnerNiNumber = employmentCheckData.ApprenticeNiNumber;

            if (learnerNiNumber == null)
            {
                return (false, NinoFailure);
            }

            if (string.IsNullOrEmpty(learnerNiNumber.NiNumber))
            {
                return (false, NinoNotFound);
            }

            if (employmentCheckData.ApprenticeNiNumber.NiNumber.Length < validNinoLength)
            {
                return (false, NinoInvalid);
            }

            var httpStatusCode = employmentCheckData.ApprenticeNiNumber.HttpStatusCode;
            if (httpStatusCode == HttpStatusCode.NotFound || httpStatusCode == HttpStatusCode.NoContent)
            {
                return (false, NinoNotFound);
            }

            if ((int)httpStatusCode >= 400 && (int)httpStatusCode <= 599)
            {
                return (false, NinoFailure);
            }

            return (true, string.Empty);
        }

        public (bool IsValid, string ErrorType) IsValidPayeScheme(EmploymentCheckData employmentCheckData)
        {
            const string PayeNotFound = "PAYENotFound";
            const string PayeFailure = "PAYEFailure";

            if (employmentCheckData.EmployerPayeSchemes == null)
            {
                return (false, PayeFailure);
            }

            if (!employmentCheckData.EmployerPayeSchemes.PayeSchemes.Any())
            {
                return (false, PayeNotFound);
            }

            if (employmentCheckData.EmployerPayeSchemes.PayeSchemes.Any())
            {
                var emptyValue = false;
                foreach (var employerPayeScheme in employmentCheckData.EmployerPayeSchemes.PayeSchemes.Where(x => x.Length == 0))
                {
                    emptyValue = true;
                }

                if (emptyValue == true)
                {
                    return (false, PayeNotFound);
                }
            }

            var httpStatusCode = employmentCheckData.EmployerPayeSchemes.HttpStatusCode;
            if (httpStatusCode == HttpStatusCode.NotFound || httpStatusCode == HttpStatusCode.NoContent)
            {
                return (false, PayeNotFound);
            }

            if ((int)httpStatusCode >= 400 && (int)httpStatusCode <= 599)
            {
                return (false, PayeFailure);
            }

            return (true, string.Empty);
        }
    }
}
