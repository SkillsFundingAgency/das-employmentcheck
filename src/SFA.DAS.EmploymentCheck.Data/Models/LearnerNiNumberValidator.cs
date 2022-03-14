using System.Net;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public class LearnerNiNumberValidator : ILearnerNiNumberValidator
    {
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

            return (true, null);
        }
    }
}
