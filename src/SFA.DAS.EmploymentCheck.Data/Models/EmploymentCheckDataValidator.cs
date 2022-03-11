using System;
using System.Linq;
using System.Net;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public class EmploymentCheckDataValidator
    {
        private const string NinoNotFound = "NinoNotFound";
        private const string NinoInvalid = "NinoInvalid";
        private const string NinoFailure = "NinoFailure";
        private const string PayeNotFound = "PAYENotFound";
        private const string PayeFailure = "PAYEFailure";

        public bool IsValidEmploymentCheckData(EmploymentCheckData employmentCheckData)
        {
            if(employmentCheckData == null) { throw new ArgumentNullException("employmentCheckData", "EmploymentCheckDataValidator: EmploymentCheckData argument is null."); }

            var isValidEmploymentcheckData = true;
            var isValidNino = IsValidNino(employmentCheckData);
            var isValidPayeScheme = IsValidPayeScheme(employmentCheckData);

            if (!isValidNino || !isValidPayeScheme)
            {
                isValidEmploymentcheckData = false;
            }

            return isValidEmploymentcheckData;
        }

        public bool IsValidNino(EmploymentCheckData employmentCheckData)
        {
            const int validNinoLength = 9;

            if (employmentCheckData.ApprenticeNiNumber == null)
            {
                employmentCheckData.EmploymentCheck.ErrorType = NinoFailure;
                return false;
            }

            if (string.IsNullOrEmpty(employmentCheckData.ApprenticeNiNumber.NiNumber))
            {
                employmentCheckData.EmploymentCheck.ErrorType = NinoNotFound;
                return false;
            }

            if (employmentCheckData.ApprenticeNiNumber.HttpStatusCode == HttpStatusCode.NoContent)
            {
                employmentCheckData.EmploymentCheck.ErrorType = NinoNotFound;
                return false;
            }

            if (employmentCheckData.ApprenticeNiNumber.HttpStatusCode == HttpStatusCode.NotFound)
            {
                employmentCheckData.EmploymentCheck.ErrorType = NinoNotFound;
                return false;
            }

            if ((int)employmentCheckData.ApprenticeNiNumber.HttpStatusCode >= 400
                && (int)employmentCheckData.ApprenticeNiNumber.HttpStatusCode <= 599)
            {
                employmentCheckData.EmploymentCheck.ErrorType = NinoFailure;
                return false;
            }

            if (employmentCheckData.ApprenticeNiNumber.NiNumber.Length < validNinoLength)
            {
                employmentCheckData.EmploymentCheck.ErrorType = NinoInvalid;
                return false;
            }

            return true;
        }

        public bool IsValidPayeScheme(EmploymentCheckData employmentCheckData)
        {
            if (!IsValidPayeSchemeNullOrEmptyChecks(employmentCheckData))
            {
                return false;
            }

            if (employmentCheckData.EmployerPayeSchemes.HttpStatusCode == HttpStatusCode.NoContent)
            {
                employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(employmentCheckData.EmploymentCheck.ErrorType) ? PayeNotFound : $"{employmentCheckData.EmploymentCheck.ErrorType}And{PayeNotFound}";
                return false;
            }

            if (employmentCheckData.EmployerPayeSchemes.HttpStatusCode == HttpStatusCode.NotFound)
            {
                employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(employmentCheckData.EmploymentCheck.ErrorType) ? PayeNotFound : $"{employmentCheckData.EmploymentCheck.ErrorType}And{PayeNotFound}";
                return false;
            }

            if ((int)employmentCheckData.EmployerPayeSchemes.HttpStatusCode >= 400
                && (int)employmentCheckData.EmployerPayeSchemes.HttpStatusCode <= 599)
            {
                employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(employmentCheckData.EmploymentCheck.ErrorType) ? PayeFailure : $"{employmentCheckData.EmploymentCheck.ErrorType}And{PayeFailure}";
                return false;
            }

            return true;
        }

        public bool IsValidPayeSchemeNullOrEmptyChecks(EmploymentCheckData employmentCheckData)
        {
            if (employmentCheckData.EmployerPayeSchemes == null)
            {
                employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(employmentCheckData.EmploymentCheck.ErrorType) ? PayeFailure : $"{employmentCheckData.EmploymentCheck.ErrorType}And{PayeFailure}";
                return false;
            }

            if (!employmentCheckData.EmployerPayeSchemes.PayeSchemes.Any())
            {
                employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(employmentCheckData.EmploymentCheck.ErrorType) ? PayeNotFound : $"{employmentCheckData.EmploymentCheck.ErrorType}And{PayeNotFound}";
                return false;
            }

            if (employmentCheckData.EmployerPayeSchemes.PayeSchemes.Any())
            {
                var emptyValue = false;
                foreach (var employerPayeScheme in employmentCheckData.EmployerPayeSchemes.PayeSchemes.Where(x => x.Length == 0))
                {
                    emptyValue = true;  // there is no longer any validation in the 'create cache request' code so this is to stop a request being created with a 'blank' paye scheme
                }
                if (emptyValue == true)
                {
                    employmentCheckData.EmploymentCheck.ErrorType = string.IsNullOrEmpty(employmentCheckData.EmploymentCheck.ErrorType) ? PayeNotFound : $"{employmentCheckData.EmploymentCheck.ErrorType}And{PayeNotFound}";
                    return false;
                }
            }

            return true;
        }

    }
}
