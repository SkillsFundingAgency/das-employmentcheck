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

            var learnerNiNumber = employmentCheckData.ApprenticeNiNumber;

            if (learnerNiNumber == null)
            {
                return NinoFailure;
            }

            if (string.IsNullOrEmpty(learnerNiNumber.NiNumber))
            {
                return NinoNotFound;
            }

            if (employmentCheckData.ApprenticeNiNumber.NiNumber.Length < validNinoLength)
            {
                return NinoInvalid;
            }

            var httpStatusCode = employmentCheckData.ApprenticeNiNumber.HttpStatusCode;
            if (httpStatusCode == HttpStatusCode.NotFound || httpStatusCode == HttpStatusCode.NoContent)
            {
                return NinoNotFound;
            }

            if ((int)httpStatusCode >= 400 && (int)httpStatusCode <= 599)
            {
                return NinoFailure;
            }

            return null;
        }
    }
}
