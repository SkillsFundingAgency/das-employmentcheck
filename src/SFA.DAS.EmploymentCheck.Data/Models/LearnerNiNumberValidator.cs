using System.Net;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public class LearnerNiNumberValidator : ILearnerNiNumberValidator
    {
        public string NinoHasError(EmploymentCheckData employmentCheckData)
        {
            const string NinoNotFound = "NinoNotFound";
            const string NinoInvalid = "NinoInvalid";
            const string NinoFailure = "NinoFailure";
            const int validNinoLength = 9;

            if (employmentCheckData.ApprenticeNiNumber == null)
            {
                return NinoNotFound;
            }

            if (employmentCheckData.ApprenticeNiNumber.HttpStatusCode == HttpStatusCode.NoContent)
            {
                return NinoNotFound;
            }

            if ((int)employmentCheckData.ApprenticeNiNumber.HttpStatusCode >= 400 && (int)employmentCheckData.ApprenticeNiNumber.HttpStatusCode <= 599)
            {
                return NinoFailure;
            }

            if (string.IsNullOrEmpty(employmentCheckData.ApprenticeNiNumber.NiNumber))
            {
                return NinoNotFound;
            }

            if (employmentCheckData.ApprenticeNiNumber.NiNumber.Length < validNinoLength)
            {
                return NinoInvalid;
            }

            return null;
        }
    }
}
