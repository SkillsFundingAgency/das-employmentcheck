using System.Linq;
using System.Net;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public class EmployerPayeSchemesValidator : IEmployerPayeSchemesValidator
    {
        public string PayeSchemesHasError(EmploymentCheckData employmentCheckData)
        {
            const string PayeNotFound = "PAYENotFound";
            const string PayeFailure = "PAYEFailure";

            if (employmentCheckData.EmployerPayeSchemes == null)
            {
                return PayeNotFound;
            }

            if (employmentCheckData.EmployerPayeSchemes.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return PayeNotFound;
            }

            if ((int)employmentCheckData.EmployerPayeSchemes.HttpStatusCode >= 400 && (int)employmentCheckData.EmployerPayeSchemes.HttpStatusCode <= 599)
            {
                return PayeFailure;
            }

            if (employmentCheckData.EmployerPayeSchemes.PayeSchemes == null || !employmentCheckData.EmployerPayeSchemes.PayeSchemes.Any())
            {
                return PayeNotFound;
            }

            return null;
        }
    }
}
